# Spec 0010 — Fundação: Banco de Dados e Autenticação

**Status:** 🟡 Em progresso
**Data de início:** 2026-06-23
**Data de conclusão:** —
**Dependências:** Nenhuma (spec inicial)

> 📖 Esta spec implementa os alicerces do sistema descritos em [`docs/visao.md`](../visao.md). Toda a autenticação e estrutura de dados parte daqui.

---

## Objetivo

Base de dados criada com todas as tabelas principais, conexão funcional via ADO.NET, sistema de migração automática, registo e login com JWT operacionais. Esta spec é **bloqueante** para todas as restantes.

---

## Escopo

### Banco de Dados — Scripts SQL

| # | Script | Tabela | Colunas principais |
|---|---|---|---|
| 000 | `000_CriarTabelaMigration.sql` | `_Migration` | `Numero INT`, `Nome NVARCHAR(200)`, `AplicadaEm DATETIME2` |
| 001 | `001_CriarTabelaUsuarios.sql` | `Usuarios` | `id INT PK IDENTITY`, `nome NVARCHAR(100)`, `email NVARCHAR(200) UNIQUE`, `senha_hash NVARCHAR(255)`, `tipo NVARCHAR(20)` (cliente/barbeiro), `criado_em DATETIME2` |
| 002 | `002_CriarTabelaBarbearias.sql` | `Barbearias` | `id INT PK IDENTITY`, `nome NVARCHAR(150)`, `endereco NVARCHAR(300)`, `telefone NVARCHAR(20)`, `usuario_dono_id INT FK→Usuarios` |
| 003 | `003_CriarTabelaClientes.sql` | `Clientes` | `id INT PK IDENTITY`, `usuario_id INT FK→Usuarios UNIQUE`, `telefone NVARCHAR(20)` |
| 004 | `004_CriarTabelaProfissionais.sql` | `Profissionais` | `id INT PK IDENTITY`, `usuario_id INT FK→Usuarios`, `barbearia_id INT FK→Barbearias` |
| 005 | `005_CriarTabelaServicos.sql` | `Servicos` | `id INT PK IDENTITY`, `barbearia_id INT FK→Barbearias`, `nome NVARCHAR(150)`, `duracao_minutos INT`, `preco DECIMAL(10,2)` |
| 006 | `006_CriarTabelaAgendamentos.sql` | `Agendamentos` | `id INT PK IDENTITY`, `cliente_id INT FK→Clientes`, `profissional_id INT FK→Profissionais`, `servico_id INT FK→Servicos`, `data_hora_inicio DATETIME2`, `data_hora_fim DATETIME2`, `status NVARCHAR(20)` |

Todos os scripts usam `IF NOT EXISTS` para idempotência.

### Backend — Models

Arquivos a preencher em `MarcaAqui.Api/src/Models/`:

- `Usuario.cs` — id, nome, email, senha_hash, tipo, criado_em
- `Barbearia.cs` — id, nome, endereco, telefone, usuario_dono_id
- `Cliente.cs` — id, usuario_id, telefone
- `Profissional.cs` — id, usuario_id, barbearia_id
- `Servico.cs` — id, barbearia_id, nome, duracao_minutos, preco
- `Agendamento.cs` — id, cliente_id, profissional_id, servico_id, data_hora_inicio, data_hora_fim, status

### Backend — Infrastructure

| Classe | Responsabilidade |
|---|---|
| `DbConnectionFactory.cs` | Criar `SqlConnection` via connection string. Método `AplicarMigrations()` que lê scripts da pasta `Database/`, ordena por número e executa apenas os não registados em `_Migration`. Injetada como **singleton**. |
| `PasswordHasher.cs` | `Hash(string senha)` → hash BCrypt. `Verificar(string senha, string hash)` → bool. |
| `JwtTokenService.cs` | `GerarToken(Usuario usuario, int? barbeariaId)` → string JWT com claims `userId`, `tipo`, `barbearia_id`. `ValidarToken(string token)` → `ClaimsPrincipal?`. Configuração lida de `Jwt:Secret`, `Jwt:ExpirationMinutes`, `Jwt:Issuer`, `Jwt:Audience`. |
| `UsuarioRepository.cs` | `Criar(Usuario)`, `ObterPorId(int)`, `ObterPorEmail(string)`. |
| `ClienteRepository.cs` | `Criar(Cliente)`, `ObterPorUsuarioId(int)`. |
| `ProfissionalRepository.cs` | `Criar(Profissional)`, `ObterPorUsuarioId(int)`, `ObterPorBarbeariaId(int)`. |
| `AgendamentoRepository.cs` | `Criar(Agendamento)`, `ObterPorId(int)`, `ListarPorCliente(int)`, `ListarPorProfissional(int, DateTime)`. |
| `ServicoRepository.cs` | `Criar(Servico)`, `ObterPorBarbeariaId(int)`. |

### Backend — Service

| Classe | Responsabilidade |
|---|---|
| `AuthService.cs` | `Registrar(RegistroDto dto)` — valida email único, hasha senha, cria Usuario + Cliente ou Profissional. `Login(LoginDto dto)` — verifica email/senha, gera token JWT. |

### Backend — Controller

| Endpoint | Método | Descrição |
|---|---|---|
| `POST /auth/registro` | Público | Cria utilizador. Body: `RegistroDto`. Retorna 201. |
| `POST /auth/login` | Público | Autentica e retorna `TokenDto` com JWT. Retorna 200. |

### Backend — Configuração

**`Program.cs`:**
- Remover endpoint de exemplo `/weatherforecast`
- Registrar DI: `DbConnectionFactory`, `PasswordHasher`, `JwtTokenService`, `AuthService`
- Configurar autenticação JWT (`AddAuthentication().AddJwtBearer()`)
- Configurar CORS — permitir `localhost:5173` (dev) e domínio do frontend (prod)
- Configurar rate limiting — 5 tentativas/min nos endpoints `/auth/*`
- Adicionar health check `GET /health` — verifica conexão SQL Server
- Executar `AplicarMigrations()` no arranque

**`appsettings.json`:**
```json
{
  "Jwt": {
    "Secret": "__DEFINIR_VIA_USER_SECRETS__",
    "ExpirationMinutes": 480,
    "Issuer": "MarcaAqui.Api",
    "Audience": "MarcaAqui.Web"
  },
  "ConnectionStrings": {
    "DefaultConnection": "__DEFINIR_VIA_USER_SECRETS__"
  }
}
```

### DTOs

| DTO | Campos |
|---|---|
| `RegistroDto.cs` | `Nome`, `Email`, `Senha`, `Tipo` (cliente/barbeiro) |
| `LoginDto.cs` | `Email`, `Senha` |
| `TokenDto.cs` | `Token`, `ExpiraEm`, `Tipo` |

---

## Verificação

- [ ] `POST /auth/registro` cria utilizador e retorna 201
- [ ] `POST /auth/registro` com email duplicado retorna 409
- [ ] `POST /auth/login` com credenciais válidas retorna token JWT + claims `userId`, `tipo`, `barbearia_id`
- [ ] `POST /auth/login` com credenciais inválidas retorna 401
- [ ] Endpoint protegido rejeita requisição sem token (401)
- [ ] Endpoint protegido aceita requisição com token válido (200)
- [ ] `GET /health` retorna 200 com status da conexão SQL Server
- [ ] CORS permite requisições da origem do frontend configurada
- [ ] Rate limiting rejeita a 6ª tentativa de login em 60 segundos (429)
- [ ] Scripts SQL são executados automaticamente ao iniciar a aplicação
- [ ] Tabela `_Migration` é atualizada corretamente após cada execução

---

## Subspecs

| Spec | Descrição | Status |
|---|---|---|
| — | Nenhuma subspec ainda | — |
