# 💈 MarcaAqui

**SaaS de agendamento de horários para barbearias.** O barbeiro monta a sua agenda, o cliente escolhe o serviço e marca. Simples assim.

[![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-19-61DAFB?logo=react)](https://react.dev/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-CC2927?logo=microsoftsqlserver)](https://www.microsoft.com/sql-server)
[![Status](https://img.shields.io/badge/status-em%20desenvolvimento-yellow)]()

---

## 🎯 O problema que resolvemos

Barbearias perdem clientes por depender de WhatsApp pessoal, agenda de papel ou memória do barbeiro para marcar horários. O MarcaAqui digitaliza esse fluxo — da agenda do barbeiro até à confirmação do cliente — com notificações automáticas.

---

## 👥 Dois lados, um sistema

### 💇‍♂️ Lado do Barbeiro
- Define **dias e horários de trabalho**
- Cadastra **serviços** (corte, barba, combo…)
- Configura **espaçamento entre horários** (30 min, 45 min, 1h)
- Adiciona **profissionais à equipa** (plano multi-barbeiro)
- Vê a **agenda do dia** com todos os agendamentos

### 🧑 Lado do Cliente
- Cria **conta própria** com e-mail e senha
- Acede à **página pública da barbearia**
- Escolhe **serviço + profissional + horário disponível**
- Confirma a marcação em segundos
- Recebe **lembrete WhatsApp** antes do horário

---

## 🔄 Fluxo principal

```
Barbeiro define agenda e serviços
              ↓
Cliente faz login
              ↓
Cliente acessa página da barbearia
              ↓
Cliente escolhe serviço e horário
              ↓
MarcaAqui confirma e notifica ambos via WhatsApp
```

---

## 🧱 Stack

| Camada | Tecnologia |
|---|---|
| Frontend | React 19 + Vite |
| Backend | C# (.NET 10) — ASP.NET Core Web API |
| Banco de Dados | SQL Server (ADO.NET puro, sem ORM) |
| Autenticação | JWT + BCrypt |
| Notificações | WhatsApp Business Cloud API |
| Logging | Serilog |

---

## 📁 Estrutura do Projeto

```
MarcaAqui/
├── docs/                        # Documentação do projeto
│   ├── visao.md                 # Visão do produto
│   ├── arquitetura.md           # Arquitetura técnica completa
│   ├── roadmap.md               # Roadmap de implementação (Specs)
│   ├── ADR.md                   # Registo de decisões arquiteturais
│   └── licoes_aprendidas.md     # Memória persistente da IA
├── MarcaAqui.Api/               # Backend .NET
│   └── src/
│       ├── Controllers/         # Entrada das requisições HTTP
│       ├── DTOs/                # Objetos de transferência
│       ├── Infrastructure/      # Acesso a dados, JWT, hash, WhatsApp
│       ├── Models/              # Entidades do domínio
│       ├── Service/             # Lógica de negócio
│       └── Database/            # Scripts SQL versionados
├── MarcaAqui.Web/               # Frontend React (em breve)
├── .editorconfig                # Regras de formatação
├── .gitignore                   # Exclusões do repositório
├── CLAUDE.md                    # Regras da equipa para IA
└── README.md                    # Este arquivo
```

### Padrão de camadas (Backend)

```
Controller  →  entrada HTTP, validação
    ↓
Service     →  regras de negócio
    ↓
Repository  →  acesso ao SQL Server
    ↓
SQL Server
```

---

## 🚀 Como rodar localmente

### Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [SQL Server](https://www.microsoft.com/sql-server) (local ou Docker)
- [Node.js 20+](https://nodejs.org/) (apenas quando o frontend existir)

### Backend

```bash
# 1. Subir SQL Server via Docker (opcional)
docker run -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=SuaSenha123!' -p 1433:1433 \
  mcr.microsoft.com/mssql/server:2022-latest

# 2. Configurar secrets (development)
cd MarcaAqui.Api
dotnet user-secrets set "Jwt:Secret" "chave-super-secreta-com-256-bits-minimo"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=MarcaAqui;User Id=sa;Password=SuaSenha123!;TrustServerCertificate=True"

# 3. Rodar
dotnet run
```

A API estará disponível em `http://localhost:5183` e `https://localhost:7182`.

As migrations SQL são executadas automaticamente no arranque — nenhum comando extra necessário.

---

## 🗺️ Roadmap

| Spec | Descrição | Estado |
|---|---|---|
| **0010** | Fundação: banco de dados e autenticação JWT | 🔴 Por iniciar |
| **0020** | Cadastro e gestão da barbearia | 🔴 Por iniciar |
| **0030** | Agenda do profissional e disponibilidade | 🔴 Por iniciar |
| **0040** | Marcação de horários pelo cliente | 🔴 Por iniciar |
| **0050** | Frontend React (aplicação web) | 🔴 Por iniciar |
| **0060** | Integração com WhatsApp | 🔴 Por iniciar |
| **0070** | Melhorias e polimento | 🔴 Por iniciar |

Dependências: `0010 → 0020 → 0030 → 0040 → (0050 || 0060) → 0070`

Detalhes completos em [`docs/roadmap.md`](docs/roadmap.md).

---

## 🧠 Decisões de arquitetura

Decisões chave documentadas em [`docs/ADR.md`](docs/ADR.md):

- **Monolito modular** (não microserviços)
- **ADO.NET puro** (sem ORM)
- **JWT + BCrypt** para autenticação
- **Scripts SQL versionados** (sem biblioteca de migrations)
- **Multi-tenant por `barbearia_id`** (base única partilhada)
- **React Context API** para estado global (sem Redux)

---

## 👨‍💻 Equipa

| Nome | Branch | Papel |
|---|---|---|
| **Pedro** | `devpedro` | Desenvolvedor |
| **Castor** | `castor` | Desenvolvedor |

---

## 📄 Licença

A definir.

---

> Desenvolvido com ☕ por Pedro & Castor.
