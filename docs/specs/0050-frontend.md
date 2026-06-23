# Spec 0050 — Frontend React (Aplicação Web)

**Status:** 🔴 Por iniciar
**Data de início:** —
**Data de conclusão:** —
**Dependências:** [Spec 0040 — Marcação de Horários](0040-marcacao.md)

> 📖 Esta spec implementa a interface web descrita em [`docs/visao.md`](../visao.md) — cobrindo ambos os lados do sistema (barbeiro e cliente) via React.

---

## Objetivo

Interface web completa para clientes e barbeiros. O barbeiro gere barbearia, serviços, equipa e visualiza a agenda. O cliente regista-se, faz login, escolhe barbearia, vê serviços e horários disponíveis, e marca.

---

## Escopo

### Setup do Projeto

Criar a pasta `MarcaAqui.Web/` na raiz com Vite + React 19:

```
MarcaAqui.Web/
├── index.html
├── package.json
├── vite.config.js
└── src/
    ├── main.jsx
    ├── App.jsx
    ├── api/
    │   └── apiClient.js          # Wrapper fetch com interceptor JWT
    ├── contexts/
    │   └── AuthContext.jsx        # Estado global de autenticação
    ├── components/
    │   ├── Layout.jsx
    │   ├── Navbar.jsx
    │   ├── ProtectedRoute.jsx
    │   └── ...
    └── pages/
        ├── LoginPage.jsx
        ├── RegistroPage.jsx
        ├── PainelBarbeiro/
        │   ├── MinhaBarbearia.jsx
        │   ├── ServicosPage.jsx
        │   ├── EquipaPage.jsx
        │   ├── AgendaPage.jsx
        │   └── AgendamentosDia.jsx
        ├── PaginaPublica/
        │   ├── EscolherBarbearia.jsx
        │   ├── EscolherServico.jsx
        │   ├── EscolherProfissional.jsx
        │   ├── EscolherHorario.jsx
        │   └── ConfirmarMarcacao.jsx
        └── MeusAgendamentos.jsx
```

### Configuração Base

| Ficheiro | Responsabilidade |
|---|---|
| `vite.config.js` | Proxy da API (`/api` → `http://localhost:5183`), porta 5173 |
| `src/api/apiClient.js` | Wrapper de `fetch` com: base URL da API, header `Authorization: Bearer <token>`, interceptor 401 → limpa token e redireciona para `/login` |
| `src/contexts/AuthContext.jsx` | Estado: `user`, `token`, `loading`. Métodos: `login()`, `logout()`, `registrar()`. Token persistido em `localStorage`. |
| `src/main.jsx` | Rotas React Router, Provider do AuthContext |

### Router — Rotas

| Path | Componente | Auth | Quem |
|---|---|---|---|
| `/login` | `LoginPage` | Público | Todos |
| `/registro` | `RegistroPage` | Público | Todos |
| `/painel` | `PainelBarbeiro` | Barbeiro | Redireciona para `/painel/barbearia` |
| `/painel/barbearia` | `MinhaBarbearia` | Barbeiro | Gerir dados da barbearia |
| `/painel/servicos` | `ServicosPage` | Barbeiro | CRUD de serviços |
| `/painel/equipa` | `EquipaPage` | Barbeiro | Gerir profissionais |
| `/painel/agenda` | `AgendaPage` | Barbeiro | Configurar dias/horários |
| `/painel/agendamentos` | `AgendamentosDia` | Barbeiro | Ver agendamentos do dia |
| `/barbearias/:id` | `PaginaPublica` | Cliente | Fluxo de marcação |
| `/meus-agendamentos` | `MeusAgendamentos` | Cliente | Ver e cancelar agendamentos |
| `/` | Redireciona para `/login` ou `/painel` (se barbeiro) | — | — |

### Páginas e Funcionalidades

#### 1. Login / Cadastro

- **LoginPage:** formulário email + senha. Chama `POST /auth/login`. Em caso de sucesso, armazena token, redireciona conforme `tipo` (barbeiro → `/painel`, cliente → `/barbearias`).
- **RegistroPage:** formulário nome + email + senha + tipo (cliente/barbeiro). Chama `POST /auth/registro`. Sucesso → redireciona para login com mensagem.

#### 2. Painel do Barbeiro

- **MinhaBarbearia:** se não tem barbearia, mostra formulário de criação (`POST /barbearias`). Se tem, mostra dados e formulário de edição (`PUT /barbearias/{id}`).
- **ServicosPage:** tabela de serviços + botão "Novo Serviço". Modal/form para criar/editar (`POST/PUT /barbearias/{id}/servicos`). Botão remover (`DELETE`).
- **EquipaPage:** tabela de profissionais + input para adicionar por email/ID de utilizador. Botão remover.
- **AgendaPage:** calendário semanal. Para cada dia da semana, toggle de ativo/inativo + campos hora_inicio/hora_fim. Campo de espaçamento (30/45/60 min). Salvar (`PUT /agenda/configuracao`).
- **AgendamentosDia:** seletor de data + profissional. Tabela de agendamentos do dia (`GET /agendamentos/profissional?data=`). Botão cancelar.

#### 3. Página Pública da Barbearia (Cliente)

- **EscolherBarbearia:** (futuro: busca). Por enquanto, navegação direta via URL `/barbearias/:id`.
- **EscolherServico:** lista os serviços da barbearia (`GET /barbearias/{id}/servicos`). Cliente clica num serviço.
- **EscolherProfissional:** lista os profissionais da barbearia (`GET /barbearias/{id}/profissionais`). Cliente clica num profissional.
- **EscolherHorario:** seletor de data + grelha de slots disponíveis (`GET /agenda/{profissionalId}/disponibilidade?data=`). Cliente clica num slot.
- **ConfirmarMarcacao:** resumo (serviço, profissional, data/hora) + botão "Confirmar Marcação" (`POST /agendamentos`). Sucesso → mensagem de confirmação.

#### 4. Meus Agendamentos (Cliente)

- Tabela com agendamentos futuros (`GET /agendamentos/cliente?futuros=true`).
- Cada linha: barbearia, profissional, serviço, data/hora, status, botão "Cancelar" (`DELETE /agendamentos/{id}`).

---

## Regras de UI/UX

1. **Mobile-first.** Todos os componentes devem funcionar corretamente em telas ≤ 375px de largura.
2. **Estados de loading.** Spinner/skeleton enquanto espera resposta da API.
3. **Tratamento de erros.** Mensagens de erro da API exibidas em toast ou banner. Formato `{ erro: "mensagem" }`.
4. **Confirmação.** Ações destrutivas (cancelar, remover) pedem confirmação.
5. **Português (PT-BR).** Todos os textos da interface.

---

## Dependências NPM previstas

| Pacote | Uso |
|---|---|
| `react` / `react-dom` | Core |
| `react-router-dom` | Roteamento |
| `react-hot-toast` (ou similar) | Notificações/toasts |

---

## Verificação

- [ ] Utilizador regista-se e faz login pela interface web
- [ ] Barbeiro cria/edita a sua barbearia
- [ ] Barbeiro adiciona/edita/remove serviços
- [ ] Barbeiro adiciona/remove profissionais da equipa
- [ ] Barbeiro configura dias e horários de trabalho
- [ ] Barbeiro visualiza agendamentos do dia
- [ ] Cliente navega na página pública da barbearia
- [ ] Cliente escolhe serviço → profissional → data → horário
- [ ] Cliente confirma marcação e vê mensagem de sucesso
- [ ] Cliente visualiza os seus agendamentos
- [ ] Cliente cancela um agendamento
- [ ] Sessão mantém-se com token JWT em localStorage
- [ ] Token expirado → redireciona para login (interceptor 401)
- [ ] Interface funcional em mobile (375 px)
- [ ] Estados de loading e erro visíveis ao utilizador

---

## Subspecs

| Spec | Descrição | Status |
|---|---|---|
| — | Nenhuma subspec ainda | — |
