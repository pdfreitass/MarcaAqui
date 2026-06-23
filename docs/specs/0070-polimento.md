# Spec 0070 — Melhorias e Polimento

**Status:** 🔴 Por iniciar
**Data de início:** —
**Data de conclusão:** —
**Dependências:** [Spec 0050 — Frontend React](0050-frontend.md) + [Spec 0060 — WhatsApp](0060-whatsapp.md)

> 📖 Esta spec implementa melhorias transversais descritas em [`docs/visao.md`](../visao.md) e [`docs/arquitetura.md`](../arquitetura.md) — validações, logging, testes, fuso horário, e funcionalidades complementares.

---

## Objetivo

Elevar a qualidade geral do sistema com logging estruturado, testes, tratamento de fuso horário, paginação, upload de fotos e melhorias de segurança. Esta spec não introduz novas funcionalidades de negócio — polimento e robustez.

---

## Escopo

### 1. Validação e Tratamento de Erros Global

- [ ] Criar middleware global de exceção (`ExceptionMiddleware`) que captura exceções não tratadas
- [ ] Formato de erro padronizado: `{ erro: "mensagem" }`
- [ ] Mapeamento de exceções comuns para HTTP status codes:
  - `ArgumentException` / `ValidationException` → 400
  - `UnauthorizedAccessException` → 401/403
  - `KeyNotFoundException` → 404
  - `InvalidOperationException` (conflito) → 409
  - Outras → 500 (com log de erro via Serilog)
- [ ] Validação de DTOs com Data Annotations (`[Required]`, `[EmailAddress]`, `[MinLength]`, `[MaxLength]`, `[Range]`)

### 2. Logging Estruturado com Serilog

- [ ] Adicionar pacote NuGet `Serilog.AspNetCore`
- [ ] Configurar Serilog no `Program.cs`
- [ ] Sinks:
  - **Console** (dev): formato legível
  - **Ficheiro rolante** (prod): `logs/marcaaqui-{Data}.log`, formato JSON
- [ ] Enriquecimento com `userId` e `barbearia_id` de cada request (via middleware/enricher)
- [ ] Níveis: `Information` (fluxos normais), `Warning` (recuperável), `Error` (exceções)
- [ ] Substituir `ILogger` padrão por Serilog

### 3. Testes Unitários

- [ ] Projeto de testes: `MarcaAqui.Testes/` com xUnit
- [ ] Cobrir serviços principais:
  - `AuthService` — registo, login, email duplicado, senha inválida
  - `AgendaService` — cálculo de slots, conflitos, espaçamento
  - `AgendamentoService` — criar, cancelar, validação de conflito
- [ ] Mocks para `DbConnectionFactory`, `PasswordHasher`, `JwtTokenService`, repositórios
- [ ] Sem dependência de base de dados real

### 4. Testes de Integração

- [ ] Testes end-to-end com `WebApplicationFactory` (ASP.NET Core TestServer)
- [ ] Base de dados em memória ou SQL Server local de teste
- [ ] Fluxos completos:
  - Registo → login → criar barbearia → adicionar serviços → configurar agenda
  - Cliente regista → login → vê barbearia → escolhe serviço → marca → cancela

### 5. Tratamento de Fuso Horário

- [ ] Todas as datas armazenadas em **UTC** no banco de dados
- [ ] Conversão para fuso local no frontend (React)
- [ ] API aceita e retorna datas em ISO 8601 com offset (ex: `2026-07-01T14:00:00-03:00`)
- [ ] Configuração de fuso horário por barbearia (campo `fuso_horario NVARCHAR(50)` na tabela `Barbearias`). Script: `010_AdicionarFusoHorarioBarbearias.sql`
- [ ] Cálculo de slots disponíveis considera o fuso da barbearia

### 6. Paginação

- [ ] Adicionar paginação a todos os endpoints de listagem:
  - `GET /agendamentos/cliente?pagina=1&tamanho=20`
  - `GET /agendamentos/profissional?data=&pagina=1&tamanho=20`
  - `GET /barbearias/{id}/servicos?pagina=1&tamanho=50`
  - `GET /barbearias/{id}/profissionais?pagina=1&tamanho=50`
- [ ] Resposta paginada: `{ items: [...], total: N, pagina: 1, tamanho: 20 }`
- [ ] Classe genérica `ResultadoPaginado<T>` para padronizar

### 7. Bloqueio de Horários Específicos

- [ ] Nova tabela `BloqueiosHorario` (script `011_CriarTabelaBloqueiosHorario.sql`):
  - `id INT PK IDENTITY`, `profissional_id INT FK`, `data_hora_inicio DATETIME2`, `data_hora_fim DATETIME2`, `motivo NVARCHAR(300)`
- [ ] Endpoints:
  - `POST /agenda/{profissionalId}/bloqueios` — bloquear horário (férias, folga)
  - `DELETE /agenda/{profissionalId}/bloqueios/{id}` — remover bloqueio
- [ ] `ObterHorariosDisponiveis` considera bloqueios como slots indisponíveis

### 8. Upload de Foto de Perfil

- [ ] Endpoints:
  - `POST /barbearias/{id}/foto` — upload de foto da barbearia (multipart/form-data)
  - `POST /profissionais/{id}/foto` — upload de foto do profissional
- [ ] Validações: max 2 MB, formatos JPG/PNG/WebP
- [ ] Armazenamento: `wwwroot/uploads/{tipo}_{id}_{guid}.{ext}`
- [ ] Servir como ficheiro estático via ASP.NET Core
- [ ] Coluna `foto_url NVARCHAR(500)` nas tabelas `Barbearias` e `Profissionais`. Script: `012_AdicionarFotoUrl.sql`

### 9. Responsividade Mobile-First (confirmação)

- [ ] Verificar todos os componentes React em viewport ≤ 375 px
- [ ] Ajustes de CSS onde necessário (grid, flex, font-size)
- [ ] Testar em Chrome DevTools device emulation (iPhone SE, Galaxy S9)

---

## Verificação

- [ ] Erros da API retornam envelope `{ erro: "mensagem" }` consistente
- [ ] Logs estruturados aparecem no console (dev) e ficheiro (prod)
- [ ] Logs incluem `userId` e `barbearia_id` do request
- [ ] Testes unitários passam cobrindo AuthService, AgendaService, AgendamentoService
- [ ] Testes de integração cobrem fluxo completo registo → marcação → cancelamento
- [ ] Datas são armazenadas em UTC e convertidas para fuso local no frontend
- [ ] Endpoints de listagem suportam paginação
- [ ] Barbeiro consegue bloquear horários específicos (folgas)
- [ ] Barbeiro faz upload de foto de perfil da barbearia
- [ ] Profissional faz upload de foto de perfil
- [ ] Interface funciona corretamente em dispositivos móveis (≤ 375 px)
- [ ] Fotos são servidas como ficheiro estático

---

## Subspecs

| Spec | Descrição | Status |
|---|---|---|
| — | Nenhuma subspec ainda | — |
