# ADR — Architecture Decision Records

Registo de decisões arquiteturais significativas. Cada ADR documenta o contexto, as alternativas consideradas e as consequências da decisão. Este documento serve como memória técnica e evita que decisões passadas sejam questionadas ou revertidas sem novos factos.

Formato inspirado em [Michael Nygard](https://cognitect.com/blog/2011/11/15/documenting-architecture-decisions).

---

## ADR-001: Monolito Modular (não Microserviços)

**Data:** 2026-06-10
**Estado:** Aceite

### Contexto

O sistema precisa de servir múltiplas barbearias com isolamento de dados. A stack escolhida é .NET + React + SQL Server. É necessário decidir a topologia de deployment.

### Decisão

Arquitetura **monolito modular** — uma única aplicação .NET com separação interna em camadas (Controllers → Services → Repositories), deploy como uma unidade.

### Alternativas consideradas

| Alternativa | Motivo da rejeição |
|---|---|
| Microserviços (ex: um serviço por domínio: auth, agendamentos, notificações) | Complexidade operacional desnecessária para o volume esperado. Orquestração, service discovery, comunicação entre serviços — custo elevado sem ganho real nesta fase. |
| Serverless (Azure Functions / AWS Lambda) | Vendor lock-in, cold starts, debugging mais difícil. |

### Consequências

- **Positivas:** Deploy simples (um artefato), debugging direto, menor custo de infraestrutura.
- **Negativas:** Se uma funcionalidade saturar, toda a aplicação escala junta. Mitigação futura: isolar módulos específicos em serviços separados se houver necessidade real de escala independente.

---

## ADR-002: ADO.NET Puro (sem ORM)

**Data:** 2026-06-10
**Estado:** Aceite

### Contexto

É necessário aceder ao SQL Server para operações CRUD. A stack .NET oferece múltiplas opções: Entity Framework Core, Dapper, ADO.NET puro.

### Decisão

**ADO.NET puro** com `SqlConnection` e queries SQL escritas manualmente. A classe `DbConnectionFactory` gere a criação de conexões.

### Alternativas consideradas

| Alternativa | Motivo da rejeição |
|---|---|
| Entity Framework Core | Abstração pesada, geração de SQL nem sempre previsível, curva de aprendizagem do LINQ para queries complexas, migrações automáticas difíceis de auditar. |
| Dapper | Bom meio-termo, mas adiciona uma dependência externa sem necessidade crítica. O mapeamento manual é simples para o número de entidades previsto. |

### Consequências

- **Positivas:** Controlo total sobre o SQL, performance previsível, sem dependências extra.
- **Negativas:** Mais código boilerplate para mapear `SqlDataReader` → objetos. Sem compile-time safety nas queries.

---

## ADR-003: JWT para Autenticação

**Data:** 2026-06-10
**Estado:** Aceite

### Contexto

A API REST precisa de autenticação stateless para clientes e barbeiros. É necessário transmitir identidade e permissões a cada requisição.

### Decisão

**JWT (JSON Web Token)** emitido no login, verificado a cada requisição. Claims incluídas: `userId`, `tipo` (cliente/barbeiro), `barbearia_id`. Hash de senhas com bcrypt (`PasswordHasher`).

### Alternativas consideradas

| Alternativa | Motivo da rejeição |
|---|---|
| Sessões (cookies + estado no servidor) | Stateful — complica escalabilidade horizontal, não é idiomático para APIs REST. |
| OAuth2 / OpenID Connect com provider externo (Google, Facebook) | Complexidade adicional. Pode ser adicionado no futuro como opção de login social, mas não como mecanismo primário. |
| API Keys | Não escala para múltiplos utilizadores com permissões granulares. |

### Consequências

- **Positivas:** Stateless, simples de implementar, bem suportado no ecossistema .NET e React.
- **Negativas:** Revogação de token não é imediata (token vive até expirar). Mitigação: tempo de vida curto (8h) e rotação de chave se necessário.

---

## ADR-004: Scripts SQL Versionados (sem Biblioteca de Migrations)

**Data:** 2026-06-10
**Estado:** Aceite

### Contexto**

O schema do banco de dados evolui com o tempo. É necessário um mecanismo para aplicar mudanças de forma ordenada e repetível.

### Decisão

Scripts SQL numerados sequencialmente (`001`, `002`, …), executados por ordem. Tabela de controlo `_Migration` regista quais já foram aplicados. Método `AplicarMigrations()` no `DbConnectionFactory` executa os scripts pendentes no arranque.

### Alternativas consideradas

| Alternativa | Motivo da rejeição |
|---|---|
| Entity Framework Migrations | Só faz sentido com EF Core, que foi rejeitado no ADR-002. |
| DbUp / FluentMigrator | Adicionam dependência externa. O mecanismo manual é trivial para o número de tabelas previsto. |

### Consequências

- **Positivas:** Zero dependências, scripts SQL auditáveis e versionáveis no repositório, sem caixa-preta.
- **Negativas:** Sem rollback automático. Em caso de falha, é necessário script manual de reversão.

---

## ADR-005: React com Vite para Frontend

**Data:** 2026-06-10
**Estado:** Aceite

### Contexto

O sistema precisa de uma interface web para clientes e barbeiros. É necessário escolher framework e ferramenta de build.

### Decisão

**React JS** com **Vite** como bundler. Escolha do Castor (dono do produto).

### Alternativas consideradas

| Alternativa | Motivo da rejeição |
|---|---|
| Angular | Curva de aprendizagem mais íngreme, excessivo para o escopo. |
| Vue.js | Boa alternativa, mas React foi a preferência do Castor. |
| Blazor | Acopla o frontend ao ecossistema .NET, dificulta deploy independente e contratação de frontend developers. |
| Create React App | CRA está deprecated. Vite é mais rápido e moderno. |

### Consequências

- **Positivas:** Ecossistema vasto, Vite é rápido, SPA bem adequada ao modelo de interação.
- **Negativas:** SEO não é prioritário (SPA), mas não é relevante para um SaaS com área logada.

---

## ADR-006: React Context API (sem Redux)

**Data:** 2026-06-10
**Estado:** Aceite

### Contexto

O frontend React precisa de estado global para autenticação (token JWT, dados do utilizador).

### Decisão

**React Context API** nativa, sem bibliotecas externas de gestão de estado.

### Alternativas consideradas

| Alternativa | Motivo da rejeição |
|---|---|
| Redux / Zustand | O estado global do MarcaAqui é trivial — apenas `AuthContext` com token e dados do utilizador. Redux introduziria boilerplate desproporcional. |
| Zustand | Bom candidato se o estado global crescer. Pode ser adotado no futuro sem grande custo de migração. |

### Consequências

- **Positivas:** Zero dependências, simples, suficiente para o escopo atual.
- **Negativas:** Context API pode causar re-renders desnecessários se não for bem particionada. Mitigação: contexts separados por domínio se surgir mais estado global.

---

## ADR-007: Multi-Tenancy por `barbearia_id` (Base Partilhada)

**Data:** 2026-06-10
**Estado:** Aceite

### Contexto

O sistema serve múltiplas barbearias independentes. É necessário isolar os dados de cada uma.

### Decisão

Base de dados única com isolamento lógico via coluna `barbearia_id`. Todas as queries que acedem a dados de uma barbearia filtram por `barbearia_id` obtido do utilizador autenticado.

### Alternativas consideradas

| Alternativa | Motivo da rejeição |
|---|---|
| Base de dados por barbearia (database-per-tenant) | Complexidade operacional (N connection strings, backups separados, migrations em cada base). Excesso para o estágio inicial. |
| Schema por barbearia (schema-per-tenant) | Funciona no SQL Server mas complica migrations e ferramentas. |

### Consequências

- **Positivas:** Simplicidade — uma connection string, uma migration, queries diretas.
- **Negativas:** Risco de leak de dados entre barbearias se um developer esquecer o filtro `WHERE barbearia_id = @barbearia_id`. Mitigação: o `barbearia_id` é obtido do token JWT (fonte confiável), não de input do utilizador.

---

## ADR-008: Disco Local para Uploads

**Data:** 2026-06-10
**Estado:** Aceite

### Contexto**

O sistema precisa de armazenar fotos de perfil de profissionais e barbearias.

### Decisão

Armazenamento em disco local no servidor, pasta `wwwroot/uploads/`, servido como ficheiro estático. Limite de 2 MB por ficheiro, formatos JPG/PNG/WebP.

### Alternativas consideradas

| Alternativa | Motivo da rejeição |
|---|---|
| Azure Blob Storage / AWS S3 | Custo adicional, complexidade de configuração. Volume de uploads é baixo (só fotos de perfil). |
| Base de dados (BLOB/VARBINARY) | Degrada performance do SQL Server, backups ficam pesados. |

### Consequências

- **Positivas:** Simples, zero custo adicional, zero latência de rede externa.
- **Negativas:** Não escala horizontalmente (várias instâncias não partilham disco). Mitigação: se houver necessidade de escala horizontal, migrar para Blob Storage com adaptador — o contrato de armazenamento pode ser abstraído numa interface.

---

## ADR-009: `BackgroundService` para Lembretes (sem Hangfire/Quartz)

**Data:** 2026-06-10
**Estado:** Aceite

### Contexto

O sistema precisa de enviar lembretes WhatsApp antes dos agendamentos (ex: 1h antes, 1 dia antes). É necessário um mecanismo de agendamento e execução periódica.

### Decisão

`BackgroundService` nativo do .NET com loop que verifica a cada minuto os agendamentos próximos e dispara envio.

### Alternativas consideradas

| Alternativa | Motivo da rejeição |
|---|---|
| Hangfire / Quartz.NET | Adicionam dependência externa e exigem storage (Hangfire precisa de tabelas no SQL Server). Excesso para um caso de uso simples. |
| Azure Functions / AWS Lambda com timer trigger | Vendor lock-in, complexidade de desenvolvimento local. |

### Consequências

- **Positivas:** Nativo do .NET, zero dependências, simples de depurar.
- **Negativas:** Sem retry automático sofisticado, sem dashboard de jobs. Se as necessidades de background jobs crescerem, migrar para Hangfire.

---

## ADR-010: Sem Versionamento de API (por enquanto)

**Data:** 2026-06-10
**Estado:** Aceite

### Contexto

APIs REST precisam de estratégia de versionamento para evitar breaking changes em clientes ativos. Nesta fase inicial, o único cliente é o frontend React desenvolvido em paralelo.

### Decisão

Sem versionamento explícito na URL. Uma única versão ativa da API. URLs sem prefixo `/v1/`.

### Alternativas consideradas

| Alternativa | Motivo da rejeição |
|---|---|
| `/v1/` no path | Custo de manutenção sem benefício real enquanto só existe um cliente (o frontend próprio). |
| Header `Accept: application/vnd.marcaaqui.v1+json` | Complexo, difícil de testar com ferramentas simples. |

### Consequências

- **Positivas:** URLs mais limpas, menos código.
- **Negativas:** Se houver clientes externos no futuro, breaking changes serão mais difíceis de gerir. Mitigação: adotar `/v1/` no path quando surgir o primeiro cliente externo.

---

## ADR-011: Serilog para Logging Estruturado

**Data:** 2026-06-10
**Estado:** Aceite

### Contexto

A aplicação precisa de logging para diagnóstico em produção. O logging padrão do ASP.NET Core (`ILogger`) é funcional mas limitado em sinks e enriquecimento.

### Decisão

**Serilog** como biblioteca de logging. Sinks: console (dev) + ficheiro rolante (prod). Enriquecimento com `userId` e `barbearia_id` de cada request.

### Alternativas consideradas

| Alternativa | Motivo da rejeição |
|---|---|
| NLog | Funcionalmente equivalente, Serilog tem sintaxe mais fluente e melhor integração com ASP.NET Core. |
| `ILogger` padrão sem Serilog | Faltam sinks de ficheiro integrados, enriquecimento é mais verboso. |

### Consequências

- **Positivas:** Logging estruturado (JSON), múltiplos sinks, enriquecimento simples via middleware.
- **Negativas:** Uma dependência externa adicional.

---

## ADR-012: WhatsApp Business Cloud API (não soluções de terceiros)

**Data:** 2026-06-10
**Estado:** Aceite

### Contexto

O sistema precisa de enviar notificações WhatsApp: lembretes ao cliente, avisos de novo agendamento e cancelamento ao barbeiro.

### Decisão

**WhatsApp Business Cloud API** oficial da Meta, via `IWhatsAppService`.

### Alternativas consideradas

| Alternativa | Motivo da rejeição |
|---|---|
| Twilio / MessageBird (intermediários) | Camada extra com custo adicional, sem ganho de funcionalidade relevante. |
| WhatsApp Web não oficial (bibliotecas como `whatsapp-web.js`) | Violação dos ToS do WhatsApp, risco de banimento, sem suporte a templates. |
| SMS em vez de WhatsApp | WhatsApp é o canal preferido do público-alvo (barbeiros e clientes no Brasil/Portugal). |

### Consequências

- **Positivas:** API oficial, confiável, suporte a templates aprovados.
- **Negativas:** Exige aprovação de templates e verificação do número business pela Meta. Custo por mensagem (modelo de conversa).

---

## Resumo

| ADR | Decisão | Princípio |
|---|---|---|
| 001 | Monolito modular | KISS |
| 002 | ADO.NET puro | Controlo |
| 003 | JWT + bcrypt | Simplicidade |
| 004 | Scripts SQL versionados | Zero dependências |
| 005 | React + Vite | Escolha do Castor |
| 006 | Context API (sem Redux) | KISS |
| 007 | Multi-tenant por `barbearia_id` | Simplicidade |
| 008 | Disco local para uploads | KISS |
| 009 | BackgroundService nativo | Zero dependências |
| 010 | Sem versionamento de API | KISS |
| 011 | Serilog | Observabilidade |
| 012 | WhatsApp Cloud API | Oficial / Confiável |

> **Nota:** Este documento deve ser atualizado sempre que uma nova decisão arquitetural significativa for tomada. ADRs antigas não são alteradas — se uma decisão for revertida, cria-se um novo ADR que referencia o anterior.
