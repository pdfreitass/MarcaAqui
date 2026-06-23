# Roadmap - MarcaAqui

Roadmap de implementação do sistema MarcaAqui. Cada spec representa uma etapa autónoma e verificável. A numeração segue o padrão do projeto: specs principais de 10 em 10 (`0010`, `0020`, …); correções e hotfixes acrescentam 1 (`0011`, `0012`, …).

A documentação detalhada de cada spec está na pasta [`docs/specs/`](specs/).

---

## 📊 Progresso Geral

```
⚪⚪⚪⚪⚪⚪⚪ 0%  (0 de 7 specs concluídas)
```

| Spec | Estado |
|---|---|
| [0010 — Fundação](specs/0010-fundacao.md) | 🔴 Por iniciar |
| [0020 — Gestão da Barbearia](specs/0020-gestao-barbearia.md) | 🔴 Por iniciar |
| [0030 — Agenda do Profissional](specs/0030-agenda.md) | 🔴 Por iniciar |
| [0040 — Marcação de Horários](specs/0040-marcacao.md) | 🔴 Por iniciar |
| [0050 — Frontend React](specs/0050-frontend.md) | 🔴 Por iniciar |
| [0060 — Integração WhatsApp](specs/0060-whatsapp.md) | 🔴 Por iniciar |
| [0070 — Melhorias e Polimento](specs/0070-polimento.md) | 🔴 Por iniciar |

> ⚪ = por iniciar &nbsp;&nbsp; 🟡 = em progresso &nbsp;&nbsp; 🟢 = concluída

---

## 📋 Spec 0010 — Fundação: Banco de Dados e Autenticação

📄 Documento completo: [`docs/specs/0010-fundacao.md`](specs/0010-fundacao.md)

**Objetivo:** Base de dados criada, conexão funcional, registo e login com JWT operacionais.

### Itens

- [ ] `000_CriarTabelaMigration.sql` — tabela `_Migration` (Numero, Nome, AplicadaEm) para controlo de versão
- [ ] `001_CriarTabelaUsuarios.sql` — tabela `Usuarios` (id, nome, email, senha_hash, tipo, criado_em)
- [ ] `002_CriarTabelaBarbearias.sql` — tabela `Barbearias` (id, nome, endereco, telefone, usuario_dono_id)
- [ ] `003_CriarTabelaClientes.sql` — tabela `Clientes` (id, usuario_id, telefone)
- [ ] `004_CriarTabelaProfissionais.sql` — tabela `Profissionais` (id, usuario_id, barbearia_id)
- [ ] `005_CriarTabelaServicos.sql` — tabela `Servicos` (id, barbearia_id, nome, duracao_minutos, preco)
- [ ] `006_CriarTabelaAgendamentos.sql` — tabela `Agendamentos` (id, cliente_id, profissional_id, servico_id, data_hora_inicio, data_hora_fim, status)

### Backend

- [ ] Implementar `Models/` — classes de domínio (Usuario, Barbearia, Cliente, Profissional, Servico, Agendamento)
- [ ] Implementar `Infrastructure/DbConnectionFactory.cs` — fábrica de conexão SQL Server via ADO.NET, incluindo método `AplicarMigrations()` que lê e executa scripts SQL por ordem
- [ ] Implementar `Infrastructure/PasswordHasher.cs` — hash e verificação de senha (bcrypt)
- [ ] Implementar `Infrastructure/JwtTokenService.cs` — geração e validação de tokens JWT. Claims: `userId`, `tipo`, `barbearia_id`
- [ ] Criar `Infrastructure/UsuarioRepository.cs` — repositório base (métodos `Criar`, `ObterPorId`, `ObterPorEmail`, etc.)
- [ ] Implementar `Infrastructure/*Repository.cs` existentes — preencher os repositórios já criados na scaffold
- [ ] Implementar `Service/AuthService.cs` — lógica de registo e login
- [ ] Implementar `Controllers/AuthController.cs` — endpoints `POST /auth/registro` e `POST /auth/login`
- [ ] Adicionar chaves de configuração JWT ao `appsettings.json`: `Jwt:Secret`, `Jwt:ExpirationMinutes`, `Jwt:Issuer`, `Jwt:Audience`
- [ ] Adicionar `ConnectionStrings:DefaultConnection` ao `appsettings.json`
- [ ] Configurar DI (DbConnectionFactory, PasswordHasher, JwtTokenService, AuthService) no `Program.cs`
- [ ] Configurar middleware de autenticação JWT no `Program.cs`
- [ ] Configurar CORS no `Program.cs` — permitir `localhost:5173` (dev) e domínio do frontend (prod)
- [ ] Configurar rate limiting no `Program.cs` — 5 tentativas/min nos endpoints `/auth/*`
- [ ] Adicionar health check `GET /health` no `Program.cs` — verifica conexão SQL Server
- [ ] Remover endpoint de exemplo `/weatherforecast` do `Program.cs`

### DTOs envolvidos

- [ ] `RegistroDto.cs`, `LoginDto.cs`, `TokenDto.cs`

### Verificação

- `POST /auth/registro` cria utilizador e retorna 201
- `POST /auth/login` retorna token JWT válido com claims `userId`, `tipo` e `barbearia_id`
- Endpoint protegido rejeita requisição sem token (401)
- Endpoint protegido aceita requisição com token válido (200)
- `GET /health` retorna 200 com status da conexão SQL Server
- CORS permite requisições da origem do frontend
- Rate limiting rejeita a 6ª tentativa de login em 60 segundos (429)
- Scripts SQL são executados automaticamente ao iniciar a aplicação (tabela `_Migration` atualizada)

---

## Spec 0020 — Cadastro e Gestão da Barbearia

📄 Documento completo: [`docs/specs/0020-gestao-barbearia.md`](specs/0020-gestao-barbearia.md)

**Objetivo:** Barbeiro consegue criar a sua barbearia, gerir serviços e adicionar profissionais à equipa.

**Dependência:** Spec 0010

### Itens

- [ ] Criar `Infrastructure/BarbeariaRepository.cs` — CRUD de barbearia
- [ ] Implementar `Service/BarbeariaService.cs` — CRUD de barbearia (criar, editar, obter por dono)
- [ ] Implementar `Controllers/BarbeariaController.cs` — endpoints protegidos para gestão da barbearia
- [ ] Implementar `Service/ServicoService.cs` — CRUD de serviços vinculados à barbearia
- [ ] Implementar `Controllers/ServicoController.cs` — endpoints protegidos para gestão de serviços
- [ ] Implementar `Service/ProfissionalService.cs` — CRUD de profissionais vinculados à barbearia
- [ ] Implementar `Controllers/ProfissionalController.cs` — endpoints protegidos para gestão de profissionais

### DTOs envolvidos

- [ ] `BarbeariaDto.cs`, `ServicoDto.cs`, `ProfissionalDto.cs`

### Verificação

- Barbeiro autenticado cria/edita a sua barbearia
- Barbeiro adiciona/remove/edita serviços vinculados à sua barbearia
- Barbeiro adiciona/remove profissionais da equipa (plano equipa)
- Barbeiro A não consegue aceder à barbearia do barbeiro B

---

## Spec 0030 — Agenda do Profissional

📄 Documento completo: [`docs/specs/0030-agenda.md`](specs/0030-agenda.md)

**Objetivo:** Cada profissional define a sua disponibilidade semanal e o sistema calcula grelha de horários disponíveis.

**Dependência:** Spec 0020

### Itens

- [ ] Criar tabela `HorariosTrabalho` (script SQL `007_*`) — profissional_id, dia_semana, hora_inicio, hora_fim
- [ ] Criar tabela `ConfiguracaoAgenda` (script SQL `008_*`) — profissional_id, espacamento_minutos
- [ ] Implementar `Models/HorarioTrabalho.cs` e `Models/ConfiguracaoAgenda.cs`
- [ ] Criar `Infrastructure/AgendaRepository.cs` — acesso às tabelas `HorariosTrabalho` e `ConfiguracaoAgenda`
- [ ] Implementar `Service/AgendaService.cs` — definir horários de trabalho, espaçamento e método `ObterHorariosDisponiveis(profissionalId, data)` que calcula slots livres considerando agendamentos existentes
- [ ] Implementar `Controllers/AgendaController.cs` — endpoints:
  - `PUT /agenda/configuracao` — define dias e espaçamento
  - `GET /agenda/{profissionalId}/disponibilidade?data=YYYY-MM-DD` — retorna slots disponíveis

### DTOs envolvidos

- [ ] `ConfiguracaoAgendaDto.cs`, `HorarioDisponivelDto.cs`

### Verificação

- Profissional define dias de trabalho (ex: seg-sex) e horário (ex: 09:00-18:00)
- Profissional define espaçamento (ex: 30 min)
- `GET /disponibilidade` retorna lista de slots livres para uma data
- Slots já agendados não aparecem como disponíveis
- Slots respeitam o espaçamento configurado

---

## Spec 0040 — Marcação de Horários (Cliente)

📄 Documento completo: [`docs/specs/0040-marcacao.md`](specs/0040-marcacao.md)

**Objetivo:** Cliente autenticado visualiza serviços, escolhe horário disponível e confirma agendamento.

**Dependência:** Spec 0030

### Itens

- [ ] Implementar `Service/ClienteService.cs` — completar perfil do cliente (associar a utilizador)
- [ ] Implementar `Controllers/ClienteController.cs` — endpoint para completar cadastro do cliente
- [ ] Implementar `Service/AgendamentoService.cs` — criar agendamento com validações:
  - Conflito de horário (slot já ocupado)
  - Serviço pertence à barbearia do profissional
  - Cliente e profissional existem
- [ ] Implementar `Service/AgendamentoService.cs` — listar agendamentos do cliente
- [ ] Implementar `Service/AgendamentoService.cs` — cancelar agendamento (cliente ou barbeiro)
- [ ] Implementar `Controllers/AgendamentoController.cs` — endpoints:
  - `POST /agendamentos` — criar marcação
  - `GET /agendamentos/cliente` — meus agendamentos (cliente)
  - `GET /agendamentos/profissional?data=YYYY-MM-DD` — agenda do dia (profissional)
  - `DELETE /agendamentos/{id}` — cancelar
- [ ] Endpoint público `GET /barbearias/{id}/servicos` — lista serviços da barbearia (sem auth)

### DTOs envolvidos

- [ ] `CriarClienteDto.cs`, `ClienteDto.cs`, `AtualizarClienteDto.cs`, `CriarAgendamentoDto.cs`, `AgendamentoDto.cs`

### Verificação

- Cliente vê serviços disponíveis da barbearia
- Cliente vê horários disponíveis de um profissional
- Cliente seleciona serviço + horário e confirma agendamento
- Sistema rejeita agendamento em slot já ocupado (409 Conflict)
- Cliente cancela agendamento e slot volta a ficar disponível
- Profissional vê a sua agenda do dia

---

## Spec 0050 — Frontend React (Aplicação Web)

📄 Documento completo: [`docs/specs/0050-frontend.md`](specs/0050-frontend.md)

**Objetivo:** Interface web completa para clientes e barbeiros.

**Dependência:** Spec 0040

### Setup

- [ ] Criar projeto `MarcaAqui.Web/` com Vite + React
- [ ] Configurar router (React Router)
- [ ] Configurar chamadas à API (fetch wrapper com intercept para JWT)
- [ ] Configurar contexto de autenticação (AuthContext)

### Páginas

- [ ] **Login / Cadastro** — formulários de registo e login
- [ ] **Painel do Barbeiro** — gerir barbearia, serviços, profissionais, agenda e ver agendamentos
- [ ] **Página pública da Barbearia** — cliente escolhe serviço, profissional, data/hora e marca
- [ ] **Meus Agendamentos** — cliente vê e cancela os seus agendamentos

### Verificação

- Utilizador regista-se e faz login via interface web
- Barbeiro configura barbearia, serviços e equipa pelo painel
- Cliente navega na página pública, escolhe serviço/horário e marca
- Cliente visualiza e cancela agendamentos
- Sessão mantém-se com token JWT armazenado

---

## Spec 0060 — Integração com WhatsApp

📄 Documento completo: [`docs/specs/0060-whatsapp.md`](specs/0060-whatsapp.md)

**Objetivo:** Notificações automáticas via WhatsApp Business API.

**Dependência:** Spec 0040

### Itens

- [ ] Implementar `Infrastructure/WhatsAppService.cs` — encapsular chamadas à WhatsApp Cloud API
- [ ] Configurar credenciais da WhatsApp Cloud API no `appsettings.json`
- [ ] Registrar `WhatsAppService` no DI
- [ ] Implementar envio de **lembrete ao cliente** (template message) — disparado X horas/dias antes do agendamento
- [ ] Implementar envio de **aviso ao barbeiro** — disparado quando um novo agendamento é criado
- [ ] Criar mecanismo de disparo (Background Service com `IHostedService` ou endpoint de callback)

### Templates previstos

| Template | Destinatário | Gatilho |
|---|---|---|
| `lembrete_agendamento` | Cliente | 1h antes / 1 dia antes do horário |
| `novo_agendamento` | Barbeiro | Novo agendamento confirmado |
| `agendamento_cancelado` | Ambos | Cancelamento de agendamento |

### Verificação

- Novo agendamento → barbeiro recebe notificação WhatsApp
- Antes do agendamento → cliente recebe lembrete WhatsApp
- Cancelamento → ambos recebem notificação WhatsApp

---

## Spec 0070 — Melhorias e Polimento

📄 Documento completo: [`docs/specs/0070-polimento.md`](specs/0070-polimento.md)

**Objetivo:** Funcionalidades complementares e qualidade geral.

**Dependência:** Spec 0050 + Spec 0060

### Itens

- [ ] Validação e tratamento de erros global (middleware de exceção, formato `{ erro: "mensagem" }`)
- [ ] Logging estruturado com Serilog (sinks: console + ficheiro, enrichment com `userId` e `barbearia_id`)
- [ ] Testes unitários para serviços principais
- [ ] Testes de integração para endpoints da API
- [ ] Tratamento de fuso horário nos agendamentos
- [ ] Paginação nos endpoints de listagem
- [ ] Bloqueio de horários específicos (feriados, folgas pontuais)
- [ ] Upload de foto de perfil (profissional e barbearia)
- [ ] Responsividade mobile-first confirmada

### Verificação

- Erros da API retornam respostas consistentes com mensagens claras
- Testes unitários cobrem AuthService, AgendaService e AgendamentoService
- Testes de integração cobrem fluxo completo registo → marcação → cancelamento
- Interface funciona corretamente em dispositivos móveis
- Horários são exibidos e tratados no fuso local correto

---

## Resumo de Dependências

```
0010 (Fundação)
  └── 0020 (Gestão Barbearia)
        └── 0030 (Agenda)
              └── 0040 (Marcação)
                    ├── 0050 (Frontend React)
                    │     └── 0070 (Polimento)
                    └── 0060 (WhatsApp)
                          └── 0070 (Polimento)
```

> **Nota:** As specs 0050 e 0060 são paralelizáveis (dependem ambas da 0040 mas não uma da outra).
