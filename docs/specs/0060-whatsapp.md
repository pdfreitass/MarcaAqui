# Spec 0060 — Integração com WhatsApp

**Status:** 🔴 Por iniciar
**Data de início:** —
**Data de conclusão:** —
**Dependências:** [Spec 0040 — Marcação de Horários](0040-marcacao.md)

> 📖 Esta spec implementa as notificações descritas em [`docs/visao.md`](../visao.md) — secção "Integração com WhatsApp". Cliente recebe lembrete antes do agendamento; barbeiro é notificado de novas marcações e cancelamentos.

---

## Objetivo

Enviar notificações automáticas via WhatsApp Business Cloud API nos seguintes eventos: novo agendamento (aviso ao barbeiro), lembrete antes do horário (cliente) e cancelamento (ambos).

---

## Escopo

### Backend — Infrastructure

| Classe | Responsabilidade |
|---|---|
| `WhatsAppService.cs` | Encapsula chamadas à WhatsApp Business Cloud API. Interface `IWhatsAppService` com métodos: `EnviarMensagem(string telefone, string templateNome, Dictionary<string, string> parametros)`. Lê configuração de `WhatsApp:ApiBaseUrl`, `WhatsApp:PhoneNumberId`, `WhatsApp:AccessToken`. |

### Backend — Service (alterações e novo)

| Classe | Responsabilidade |
|---|---|
| `AgendamentoService.cs` (alterar) | Após `CriarAgendamento` com sucesso, dispara notificação ao barbeiro via `IWhatsAppService.EnviarMensagem` (template `novo_agendamento`). Após `CancelarAgendamento`, dispara notificação a ambos (template `agendamento_cancelado`). Envio assíncrono — não bloqueia a resposta ao cliente. |
| `LembreteBackgroundService.cs` (novo) | `BackgroundService` que verifica a cada 60 segundos: agendamentos com `data_hora_inicio` daqui a 1 hora e daqui a 24 horas que ainda não receberam lembrete. Dispara `IWhatsAppService.EnviarMensagem` (template `lembrete_agendamento`). Regista na tabela `Agendamentos` que o lembrete foi enviado (novas colunas: `lembrete_1h_enviado BIT DEFAULT 0`, `lembrete_24h_enviado BIT DEFAULT 0`). |

### Banco de Dados — Alterações

| Script | Alteração |
|---|---|
| `009_AdicionarColunasLembreteAgendamentos.sql` | Adicionar colunas `lembrete_1h_enviado BIT NOT NULL DEFAULT 0` e `lembrete_24h_enviado BIT NOT NULL DEFAULT 0` à tabela `Agendamentos`. Adicionar coluna `telefone NVARCHAR(20)` à tabela `Clientes` se ainda não existir. |

### Backend — Configuração

**`appsettings.json`:**
```json
{
  "WhatsApp": {
    "ApiBaseUrl": "https://graph.facebook.com/v18.0",
    "PhoneNumberId": "__DEFINIR_VIA_USER_SECRETS__",
    "AccessToken": "__DEFINIR_VIA_USER_SECRETS__"
  }
}
```

Em desenvolvimento, usar User Secrets. Em produção, variáveis de ambiente.

### Templates WhatsApp

| Template | Destinatário | Gatilho | Parâmetros |
|---|---|---|---|
| `novo_agendamento` | Barbeiro | Novo agendamento confirmado | `cliente_nome`, `servico_nome`, `data_hora`, `profissional_nome` |
| `lembrete_agendamento` | Cliente | 1h antes / 1 dia antes | `cliente_nome`, `servico_nome`, `data_hora`, `barbearia_nome`, `profissional_nome` |
| `agendamento_cancelado` | Ambos | Cancelamento | `cliente_nome`, `servico_nome`, `data_hora` |

Os templates devem ser criados e aprovados no WhatsApp Business Manager antes da integração funcionar em produção.

---

## Regras de Negócio

1. **Envio é best-effort.** Se a chamada à WhatsApp API falhar, regista-se log de erro (Serilog) e o fluxo principal não é interrompido. Sem retry automático complexo (KISS).
2. **Não reenviar lembretes.** As colunas `lembrete_1h_enviado` e `lembrete_24h_enviado` garantem que cada lembrete é enviado apenas uma vez.
3. **Telefone do cliente.** Necessário para envio. O cliente deve ter o campo `telefone` preenchido (completar perfil — Spec 0040).
4. **Telefone do barbeiro.** Obtido da tabela `Barbearias` (campo `telefone`).
5. **Background Service não bloqueia o arranque.** O loop é envolvido em `try-catch`; erros são logados e o loop continua.
6. **Verificação periódica.** A cada 60 segundos, consulta agendamentos com status `confirmado` cuja `data_hora_inicio` está no intervalo [agora + 1h - 30s, agora + 1h + 30s] (para 1h) e [agora + 24h - 30s, agora + 24h + 30s] (para 24h), com as respetivas flags a `0`.

---

## Verificação

- [ ] Novo agendamento → barbeiro recebe notificação WhatsApp (template `novo_agendamento`)
- [ ] 1 hora antes do agendamento → cliente recebe lembrete WhatsApp (template `lembrete_agendamento`)
- [ ] 24 horas antes do agendamento → cliente recebe lembrete WhatsApp (template `lembrete_agendamento`)
- [ ] Cancelamento de agendamento → ambos recebem notificação WhatsApp (template `agendamento_cancelado`)
- [ ] Falha na chamada à API WhatsApp → erro logado, fluxo principal não interrompido
- [ ] Lembrete não é reenviado (flag `lembrete_1h_enviado` impede duplicação)
- [ ] Background Service não bloqueia o arranque da aplicação
- [ ] Templates e credenciais configuráveis via `appsettings` / User Secrets / env vars

---

## Subspecs

| Spec | Descrição | Status |
|---|---|---|
| — | Nenhuma subspec ainda | — |
