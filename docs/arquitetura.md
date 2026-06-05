# Arquitetura do Projeto - MarcaAqui

## Stack Principal

| Camada | Tecnologia |
|---|---|
| Frontend | React JS |
| Backend | C# (.NET 10) |
| Banco de Dados | SQL Server |
| Autenticação | JWT (JSON Web Token) |
| Integração | WhatsApp Business API |
| Logging | Serilog |

---

## Arquitetura Geral

O sistema segue uma arquitetura **monolito modular** com separação clara entre frontend e backend.

```
[React JS]  ←→  [.NET API REST]  ←→  [SQL Server]
     ↑                                      ↑
     └────────────── HTTPS ─────────────────┘
```

- O frontend React consome a API via HTTP/REST.
- O backend .NET expõe endpoints REST e aplica toda a lógica de negócio.
- O banco de dados SQL Server armazena todos os dados relacionais.

Optamos por monolito (e não microserviços) seguindo o princípio **KISS**. Se no futuro houver necessidade de escalar partes específicas, isolamos módulos em serviços separados.

---

## Backend (.NET API)

### Estrutura de pastas (já existente)

```
MarcaAqui.Api/
├── Controllers/     → Entrada das requisições HTTP
├── DTOs/            → Objetos de transferência (request/response)
├── Infrastructure/  → Acesso a dados, serviços externos (DB, JWT, hash)
├── Models/          → Entidades do domínio
├── Service/         → Lógica de negócio
└── Database/        → Scripts SQL versionados
```

### Padrão de camadas

```
Controller → Service → Repository → SQL Server
     ↓
   DTOs (entrada/saída)
```

- **Controller:** recebe a requisição, valida os dados, chama o Service, retorna DTO.
- **Service:** contém as regras de negócio. Não sabe detalhes de banco.
- **Repository:** acesso direto ao banco. Um por entidade principal.
- **Models:** classes simples que representam as tabelas do banco.

### Autenticação

- JWT emitido no login, verificado a cada requisição nas rotas protegidas.
- Senhas hasheadas com bcrypt (classe `PasswordHasher` já prevista).
- O token contém o `userId` e o `tipo` (barbeiro/cliente).
- **Configuração JWT:**
  - `Jwt:Secret` — chave de assinatura (256+ bits). Em dev: User Secrets. Em prod: variável de ambiente.
  - `Jwt:ExpirationMinutes` — tempo de vida do token (ex: 480 = 8h).
  - `Jwt:Issuer` — `MarcaAqui.Api`.
  - `Jwt:Audience` — `MarcaAqui.Web`.
- Rotas públicas: `POST /auth/registro`, `POST /auth/login`, `GET /barbearias/{id}/servicos`.
- Todas as restantes exigem token JWT no header `Authorization: Bearer <token>`.

### CORS

- Política configurada no `Program.cs` para permitir requisições da origem do frontend React.
- Em desenvolvimento: `http://localhost:5173` (Vite).
- Em produção: domínio do frontend (ex: `https://app.marcaaqui.com`).
- Métodos permitidos: GET, POST, PUT, DELETE.
- Headers permitidos: `Authorization`, `Content-Type`.

### Convenções da API

- **URLs:** português, plural. Ex: `/barbearias`, `/agendamentos`, `/servicos`.
- **Respostas de sucesso:** DTO direto no corpo. Sem envelope `{ data }` — simplicidade.
- **Respostas de erro:** envelope `{ erro: "mensagem" }`. Status HTTP apropriado.
- **Status comuns:** 200 (ok), 201 (criado), 400 (dados inválidos), 401 (sem token), 403 (sem permissão), 404 (não encontrado), 409 (conflito), 500 (erro interno).
- **Paginação:** query params `?pagina=1&tamanho=20`. Resposta inclui `{ items: [], total: N }` quando houver listagem paginada.
- **Versionamento:** sem versionamento explícito por enquanto (KISS). Se necessário no futuro, usar `/v1/` no path.

### Tratamento de Erros

- **Repository:** lança exceção em caso de erro (ex: `SqlException`).
- **Service:** captura exceções do Repository, traduz para erros de domínio quando relevante (ex: `throw new ConflitoException("Horário já ocupado")`).
- **Controller:** captura exceções do Service e retorna status HTTP adequado.
- **Middleware global:** captura exceções não tratadas, faz log via Serilog e retorna 500 com `{ erro: "Erro interno" }`.

### Rate Limiting

- Aplicado nos endpoints de autenticação (`/auth/*`) para proteção contra brute force.
- Limite: 5 tentativas por IP a cada 60 segundos.
- Implementado via middleware built-in do ASP.NET Core (`AddRateLimiter`).

### Health Checks

- Endpoint `GET /health` retorna 200 OK (sem auth).
- Verifica conexão com SQL Server.
- Implementado via `AddHealthChecks` do ASP.NET Core.

---

## Banco de Dados

### SGBD

SQL Server, acessado via ADO.NET puro (classe `DbConnectionFactory` já prevista).

- **Connection string:** lida da chave `ConnectionStrings:DefaultConnection` no `appsettings.json`.
- Em desenvolvimento: aponta para instância local ou Docker (`Server=localhost;Database=MarcaAqui;...`).
- Em produção: definida via variável de ambiente `ConnectionStrings__DefaultConnection` (sobrescreve o ficheiro).
- `DbConnectionFactory` injetada como singleton, cria `SqlConnection` por operação.

### Tabelas previstas

Os scripts SQL (vazios por enquanto) indicam a estrutura planejada:

| Arquivo | Tabela | Descrição |
|---|---|---|
| `001_*.sql` | Usuarios | Login, senha, tipo (cliente/profissional) |
| `002_*.sql` | Barbearias | Dados da barbearia (dono, nome, endereço) |
| `003_*.sql` | Clientes | Dados do cliente vinculado a um usuário |
| `004_*.sql` | Profissionais | Barbeiros vinculados a uma barbearia |
| `005_*.sql` | Servicos | Serviços oferecidos, duração, preço |
| `006_*.sql` | Agendamentos | Marcações feitas (cliente, serviço, data/hora) |

### Versionamento

Scripts SQL numerados em ordem de execução (`001`, `002`, `003`...).

- **Tabela de controlo:** script `000_CriarTabelaMigration.sql` cria a tabela `_Migration` que regista quais scripts já foram aplicados. Colunas: `Numero`, `Nome`, `AplicadaEm`.
- **Execução:** `DbConnectionFactory` expõe método `AplicarMigrations()` que lê a pasta `Database/`, ordena por número e executa apenas scripts ainda não registados.
- **Idempotência:** scripts usam `IF NOT EXISTS` para poderem ser reexecutados em segurança.

---

## Multi-Tenancy

O sistema serve múltiplas barbearias independentes. O isolamento é feito a nível de aplicação, não de base de dados (base única partilhada).

- **Isolamento por `barbearia_id`:** toda a query que acede a dados de uma barbearia filtra por `barbearia_id` obtido do utilizador autenticado.
- **Profissionais, Serviços e Agendamentos** estão sempre vinculados a uma `barbearia_id`.
- **Barbeiro dono:** ao autenticar-se, o `barbearia_id` é obtido da tabela `Barbearias` via `usuario_dono_id`. O `barbearia_id` é incluído como claim no token JWT para evitar queries extra.
- **Profissional de equipa:** o `barbearia_id` vem da tabela `Profissionais`.
- **Cliente:** não tem `barbearia_id` fixo — acede a qualquer barbearia pela página pública. O isolamento aplica-se apenas no momento do agendamento (serviço e profissional pertencem à barbearia).

---

## Configuração e Ambientes

### Ambientes

- **Development:** `appsettings.Development.json` + User Secrets. Conexão local, logging verboso, CORS para localhost.
- **Production:** variáveis de ambiente sobrescrevem `appsettings.json`. Connection string, JWT secret e credenciais WhatsApp vêm do ambiente.

### Segredos

- **Development:** .NET User Secrets (`dotnet user-secrets set "Jwt:Secret" "..."`). Não commitar segredos.
- **Production:** variáveis de ambiente ou cofre de segredos (Azure Key Vault / AWS Secrets Manager). Definir ao nível da infraestrutura.
- **Ficheiros `appsettings*.json`:** contêm apenas chaves não sensíveis (ex: `Jwt:ExpirationMinutes`, URLs, logging).

### Logging

- **Biblioteca:** Serilog.
- **Sinks:** console (dev) + ficheiro (prod). Ficheiro rolante por dia: `logs/marcaaqui-{Data}.log`.
- **Níveis:** `Information` para fluxos normais, `Warning` para situações recuperáveis, `Error` para exceções.
- **Contexto:** cada entrada de log inclui `userId` e `barbearia_id` do request atual (via enrichment).

---

## Frontend (React JS)

### Estrutura prevista

- Pasta separada na raiz do projeto (ex: `MarcaAqui.Web/`).
- Criado com Vite ou Create React App (a decidir na spec).
- Comunicação com a API via `fetch` ou Axios.

### Gestão de Estado e Routing

- **Routing:** React Router v6+.
- **Estado global:** React Context API (sem dependência externa — KISS).
  - `AuthContext` — utilizador autenticado, token JWT, login/logout.
- **Interceptor HTTP:** wrapper em torno de `fetch` que:
  - Anexa `Authorization: Bearer <token>` a todas as requisições.
  - Em caso de 401, limpa o token e redireciona para `/login`.
- **Estado local:** `useState` / `useReducer` nos componentes.

### Páginas principais

| Página | Quem acessa |
|---|---|
| Login / Cadastro | Cliente e Barbeiro |
| Painel do Barbeiro | Barbeiro (agenda, serviços, equipe) |
| Página pública da Barbearia | Cliente (marcar horário) |
| Meus Agendamentos | Cliente |

---

## Armazenamento de Ficheiros

- Fotos de perfil (profissional, barbearia) são os únicos ficheiros previstos.
- **Armazenamento:** disco local no servidor, pasta `wwwroot/uploads/`.
- **Acesso:** servido como ficheiro estático pelo ASP.NET Core.
- **Limite:** 2 MB por ficheiro. Formatos: JPG, PNG, WebP.
- **Nome:** `{tipo}_{id}_{guid}.{ext}` para evitar colisões (ex: `profissional_42_a1b2.jpg`).
- Se no futuro houver necessidade de escalar horizontalmente, migrar para Azure Blob Storage ou S3.

---

## Integração com WhatsApp

- API oficial do WhatsApp Business (Cloud API).
- Envio de mensagens de **lembrete** (template message) e **notificação de nova marcação**.
- Um serviço dedicado no backend (`IWhatsAppService`) será responsável por encapsular as chamadas à API.

### Templates

| Template | Destinatário | Gatilho |
|---|---|---|
| `lembrete_agendamento` | Cliente | 1h antes / 1 dia antes do horário |
| `novo_agendamento` | Barbeiro | Novo agendamento confirmado |
| `agendamento_cancelado` | Ambos | Cancelamento de agendamento |

### Processamento em Background

- **Lembretes** exigem agendamento prévio (1h/dia antes). Usar `BackgroundService` com loop que verifica a cada minuto os agendamentos próximos e dispara envio.
- **Notificações imediatas** (novo agendamento, cancelamento): disparadas no próprio request, de forma síncrona ou com retry simples em caso de falha.
- **Resiliência:** se a chamada à WhatsApp API falhar, regista log de erro. Sem fila complexa por enquanto (KISS).

### Configuração

- `WhatsApp:ApiBaseUrl` — endpoint da Cloud API (ex: `https://graph.facebook.com/v18.0`).
- `WhatsApp:PhoneNumberId` — ID do número de telefone business.
- `WhatsApp:AccessToken` — token de acesso permanente (definido via variável de ambiente em prod).

---

## Infraestrutura e Deployment

### Ambiente de Desenvolvimento

- **Backend:** `dotnet run` local. SQL Server via Docker (`docker run -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=...' -p 1433:1433 mcr.microsoft.com/mssql/server:2022-latest`).
- **Frontend:** `npm run dev` (Vite) em `localhost:5173`.

### Produção

- **Backend:** publicado como executável standalone ou container Docker. Servido via Kestrel com reverse proxy (Nginx / IIS / Azure App Service).
- **Frontend:** build estático (`npm run build`), servido via Nginx ou CDN.
- **HTTPS:** terminado no reverse proxy. Certificado via Let's Encrypt ou cloud provider.

## Decisões e escolhas

| Decisão | Motivo |
|---|---|
| Monolito, não microserviços | KISS. Escala não é problema agora. |
| ADO.NET puro, sem ORM | Simplicidade, controle total do SQL. |
| JWT para autenticação | Stateless, simples, funciona bem com API REST. |
| Scripts SQL versionados | Sem dependência de bibliotecas de migration. |
| React no frontend | Escolha do Castor. |
| WhatsApp Cloud API | Oficial, confiável, sem dependência de terceiros. |
| Serilog para logging | Leve, estruturado, fácil de configurar. |
| React Context API (sem Redux) | KISS. O estado global é simples (só auth). |
| Multi-tenant por barbearia_id | Simples, não exige base separada por cliente. |
| Disco local para uploads | KISS. Só fotos de perfil, volume baixo. |
| BackgroundService para lembretes | Nativo do .NET, sem dependência externa (ex: Hangfire). |
| Sem versionamento de API por enquanto | KISS. Uma única versão ativa. |
