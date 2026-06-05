# Visão do Projeto - MarcaAqui

## O que é o MarcaAqui

MarcaAqui é um SaaS (Software as a Service) para marcação de horários. O foco inicial é atender barbeiros, mas a arquitetura é pensada para servir qualquer profissional que trabalhe com agendamento de horários.

## Os dois lados do sistema

O sistema tem duas pontas: o **prestador** (barbeiro/dono da barbearia) e o **cliente**.

---

## Lado do Barbeiro (Prestador de Serviço)

### Configuração de agenda

- Definir quais **dias da semana** trabalha.
- Definir o **horário de início e fim** de cada dia de trabalho.
- Definir o **espaçamento entre horários** (ex: 30 min, 45 min, 1h). Isso controla de quanto em quanto tempo os clientes podem marcar.

### Cadastro de serviços

- O barbeiro cadastra os **serviços que oferece** (ex: "Corte social", "Barba", "Corte + Barba").
- Cada serviço tem um **nome** e uma **duração**.
- O cliente seleciona o serviço no momento de marcar.

### Plano Equipe (multi-barbeiro)

- O plano pode ser individual ou para equipe.
- No plano equipe, é possível **cadastrar vários barbeiros** vinculados à mesma barbearia.
- Cada barbeiro tem sua própria agenda e serviços (ou herda os da barbearia).

---

## Lado do Cliente

### Acesso

- O cliente precisa ter **login** (cadastro próprio).
- Após logado, acessa a **página pública da barbearia/prestador**.

### Marcação

- Visualiza os serviços disponíveis.
- Seleciona um serviço.
- Visualiza os **horários disponíveis** com base na agenda do barbeiro.
- Escolhe o horário e **confirma a marcação**.

---

## Integração com WhatsApp

- **Lembrete ao cliente:** notificação automática antes do horário marcado (ex: 1h antes, 1 dia antes).
- **Aviso ao barbeiro:** notificação quando um novo horário é marcado.
- Pode ser feito via API do WhatsApp Business ou solução similar.

---

## Resumo do fluxo principal

```
Barbeiro define agenda e serviços
        ↓
Cliente faz login
        ↓
Cliente acessa página da barbearia
        ↓
Cliente escolhe serviço e horário
        ↓
MarcaAqui confirma e notifica ambos (WhatsApp)
```


