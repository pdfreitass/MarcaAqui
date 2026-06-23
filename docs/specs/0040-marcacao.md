# Spec 0040 — Marcação de Horários pelo Cliente

**Status:** 🔴 Por iniciar
**Data de início:** —
**Data de conclusão:** —
**Dependências:** [Spec 0030 — Agenda do Profissional](0030-agenda.md)

> 📖 Esta spec implementa o fluxo de marcação descrito em [`docs/visao.md`](../visao.md) — secção "Lado do Cliente". O cliente navega, escolhe serviço + horário e confirma a marcação. O profissional visualiza a sua agenda do dia.

---

## Objetivo

Cliente autenticado visualiza os serviços de uma barbearia, escolhe um profissional, vê os horários disponíveis para uma data e confirma o agendamento. O sistema valida conflitos e regista a marcação. Cliente pode consultar e cancelar os seus próprios agendamentos. Profissional vê a sua agenda do dia.

---

## Escopo

### Backend — Service (novos e alterados)

| Classe | Responsabilidade |
|---|---|
| `ClienteService.cs` | `CompletarPerfil(AtualizarClienteDto dto, int usuarioId)` — adiciona telefone ao registo do cliente. `ObterPerfil(int usuarioId)` — retorna dados do cliente. |
| `AgendamentoService.cs` | `CriarAgendamento(CriarAgendamentoDto dto, int clienteId)` — valida: serviço pertence à barbearia do profissional, slot está disponível, cliente existe. `ListarAgendamentosCliente(int clienteId)` — retorna agendamentos do cliente (futuros e passados). `ListarAgendamentosProfissional(int profissionalId, DateTime data)` — agenda do dia. `CancelarAgendamento(int agendamentoId, int usuarioId, string tipoUsuario)` — cliente cancela os seus, barbeiro cancela os da sua barbearia. |

### Backend — Controller

| Endpoint | Auth | Descrição |
|---|---|---|
| `POST /agendamentos` | Cliente | Criar marcação. Body: `CriarAgendamentoDto`. Retorna 201. |
| `GET /agendamentos/cliente` | Cliente | Listar os meus agendamentos. Query: `?futuros=true`. |
| `GET /agendamentos/profissional?data=YYYY-MM-DD` | Barbeiro | Agenda do dia de um profissional da sua barbearia. |
| `DELETE /agendamentos/{id}` | Cliente/Barbeiro | Cancelar agendamento. Cliente cancela os seus; barbeiro cancela os da sua barbearia. |
| `GET /barbearias/{id}/servicos` | Público | Listar serviços de uma barbearia (já parcialmente implementado na Spec 0020). |
| `PUT /clientes/perfil` | Cliente | Completar/atualizar perfil do cliente (telefone). |
| `GET /clientes/perfil` | Cliente | Obter perfil do cliente logado. |

### DTOs envolvidos

| DTO | Campos |
|---|---|
| `CriarAgendamentoDto.cs` | `ProfissionalId`, `ServicoId`, `DataHoraInicio` (ISO 8601) |
| `AgendamentoDto.cs` | `Id`, `ClienteNome`, `ProfissionalNome`, `ServicoNome`, `DataHoraInicio`, `DataHoraFim`, `Status`, `BarbeariaNome` |
| `CriarClienteDto.cs` | `Nome`, `Email`, `Senha` |
| `AtualizarClienteDto.cs` | `Telefone` |
| `ClienteDto.cs` | `Id`, `Nome`, `Email`, `Telefone` |

---

## Regras de Negócio

1. **Conflito de horário.** Se o slot `[data_hora_inicio, data_hora_fim]` colidir com qualquer agendamento existente do mesmo profissional, retorna **409 Conflict** com mensagem `"Horário já ocupado"`.
2. **Duração do serviço.** `data_hora_fim = data_hora_inicio + duracao_minutos do serviço`. A duração pode ser diferente do espaçamento da agenda — o sistema bloqueia todo o intervalo ocupado.
3. **Serviço pertence à barbearia.** O `servico_id` deve pertencer à mesma `barbearia_id` do `profissional_id`. Caso contrário, 400.
4. **Cliente e profissional devem existir.** Validar antes de criar o agendamento.
5. **Status do agendamento.** `confirmado` ao criar, `cancelado` ao cancelar, `concluido` (uso futuro).
6. **Cancelamento.** Cliente cancela apenas os seus agendamentos. Barbeiro cancela qualquer agendamento da sua barbearia. Slot volta a ficar disponível.
7. **Agendamento no passado.** Permitir criar apenas agendamentos com `data_hora_inicio > DateTime.Now`.
8. **Último slot do dia.** Se `data_hora_fim` ultrapassar `hora_fim` do profissional, rejeitar (400).

---

## Verificação

- [ ] Cliente vê serviços disponíveis de uma barbearia (endpoint público, 200)
- [ ] Cliente vê horários disponíveis de um profissional para uma data (200)
- [ ] Cliente seleciona serviço + horário e confirma agendamento (201)
- [ ] Agendamento retorna 409 se slot já estiver ocupado
- [ ] Cliente lista os seus agendamentos (200)
- [ ] Cliente cancela um agendamento seu (200) — slot volta a ficar disponível
- [ ] Profissional visualiza a agenda do dia com todos os agendamentos (200)
- [ ] Profissional cancela um agendamento da sua barbearia (200)
- [ ] Cliente não consegue cancelar agendamento de outro cliente (403)
- [ ] Tentar agendar para um horário no passado retorna 400
- [ ] Tentar agendar serviço que não pertence à barbearia do profissional retorna 400

---

## Subspecs

| Spec | Descrição | Status |
|---|---|---|
| — | Nenhuma subspec ainda | — |
