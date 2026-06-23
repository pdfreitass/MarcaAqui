# Spec 0030 — Agenda do Profissional

**Status:** 🔴 Por iniciar
**Data de início:** —
**Data de conclusão:** —
**Dependências:** [Spec 0020 — Gestão da Barbearia](0020-gestao-barbearia.md)

> 📖 Esta spec implementa a configuração de agenda descrita em [`docs/visao.md`](../visao.md) — secção "Configuração de agenda". Cada profissional define quando trabalha e o sistema calcula slots disponíveis.

---

## Objetivo

Cada profissional define a sua disponibilidade semanal (dias e horários de trabalho) e o espaçamento entre marcações. O sistema calcula e expõe uma grelha de horários disponíveis, considerando os agendamentos já existentes.

---

## Escopo

### Banco de Dados — Novas Tabelas

| Script | Tabela | Colunas |
|---|---|---|
| `007_CriarTabelaHorariosTrabalho.sql` | `HorariosTrabalho` | `id INT PK IDENTITY`, `profissional_id INT FK→Profissionais`, `dia_semana INT` (0=Dom … 6=Sáb), `hora_inicio TIME`, `hora_fim TIME` |
| `008_CriarTabelaConfiguracaoAgenda.sql` | `ConfiguracaoAgenda` | `id INT PK IDENTITY`, `profissional_id INT FK→Profissionais UNIQUE`, `espacamento_minutos INT` (30, 45, 60) |

Único registo por profissional em `ConfiguracaoAgenda` (`UNIQUE` em `profissional_id`). Múltiplos registos em `HorariosTrabalho` (um por dia da semana).

### Backend — Models

- `HorarioTrabalho.cs` — id, profissional_id, dia_semana, hora_inicio, hora_fim
- `ConfiguracaoAgenda.cs` — id, profissional_id, espacamento_minutos

### Backend — Infrastructure

| Classe | Métodos |
|---|---|
| `AgendaRepository.cs` | `SalvarHorarios(int profissionalId, List<HorarioTrabalho>)`, `ObterHorarios(int profissionalId)`, `SalvarConfiguracao(int profissionalId, int espacamentoMinutos)`, `ObterConfiguracao(int profissionalId)`, `ObterAgendamentosDoDia(int profissionalId, DateTime data)` |

### Backend — Service

| Classe | Responsabilidade |
|---|---|
| `AgendaService.cs` | `ConfigurarAgenda(ConfiguracaoAgendaDto dto, int profissionalId, int barbeariaId)` — define horários e espaçamento. `ObterHorariosDisponiveis(int profissionalId, DateTime data)` — calcula slots livres para uma data. Algoritmo: 1) obtém horários de trabalho do dia da semana, 2) obtém agendamentos existentes, 3) particiona o período de trabalho em slots de `espacamento_minutos`, 4) remove slots que colidem com agendamentos, 5) retorna lista de `HorarioDisponivelDto`. |

### Backend — Controller

| Endpoint | Auth | Descrição |
|---|---|---|
| `PUT /agenda/configuracao` | Barbeiro | Define/atualiza dias de trabalho e espaçamento. Body: `ConfiguracaoAgendaDto`. |
| `GET /agenda/{profissionalId}/disponibilidade?data=YYYY-MM-DD` | Cliente/Barbeiro | Retorna slots disponíveis para um profissional numa data. |

### DTOs envolvidos

| DTO | Campos |
|---|---|
| `ConfiguracaoAgendaDto.cs` | `Horarios` (lista de `{ diaSemana, horaInicio, horaFim }`), `EspacamentoMinutos` |
| `HorarioDisponivelDto.cs` | `DataHoraInicio` (ISO 8601), `DataHoraFim`, `Disponivel` (bool) |

---

## Regras de Negócio

1. **Espaçamento define a duração de cada slot.** Ex: espaçamento de 30 min com trabalho das 09:00-12:00 gera slots 09:00, 09:30, 10:00, 10:30, 11:00, 11:30.
2. **Dia da semana é numérico.** 0 = Domingo, 1 = Segunda … 6 = Sábado, conforme `DayOfWeek` do .NET.
3. **Slots colidem se** o intervalo `[data_hora_inicio, data_hora_fim]` do agendamento se sobrepõe ao slot — mesmo que parcialmente. Um slot das 10:00-10:30 está ocupado se existir agendamento das 10:15-10:45.
4. **Slots passados não são retornados.** Se `data_hora_inicio < DateTime.Now`, o slot é omitido.
5. **Profissional pertence à barbearia do barbeiro.** O `barbearia_id` é validado ao configurar a agenda.

---

## Verificação

- [ ] Profissional define dias de trabalho (ex: segunda a sexta) e horário (ex: 09:00-18:00)
- [ ] Profissional define espaçamento (ex: 30 min)
- [ ] `GET /agenda/{id}/disponibilidade?data=2026-07-01` retorna lista de slots para o dia
- [ ] Slots já agendados não aparecem como disponíveis
- [ ] Slots respeitam o espaçamento configurado
- [ ] Slots com início no passado não são retornados
- [ ] Alterar configuração substitui a anterior (PUT idempotente)
- [ ] Barbeiro A não pode configurar agenda de profissional da barbearia B (403)

---

## Subspecs

| Spec | Descrição | Status |
|---|---|---|
| — | Nenhuma subspec ainda | — |
