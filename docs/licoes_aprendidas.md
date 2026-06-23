# Lições Aprendidas — MarcaAqui

Memória persistente da IA entre sessões. Toda vez que um problema for resolvido, um erro cometido ou uma abordagem se mostrar ineficaz, registramos aqui.

---

## Instruções de Uso

### Para o desenvolvedor humano

- **Quando registrar:** Após resolver qualquer bug não-trivial, após descobrir uma limitação de ferramenta/API, ou quando uma abordagem precisar ser abandonada e refeita.
- **Formato:** Use o template `LL-XXX` abaixo. Numere sequencialmente.
- **Atualize as regras rápidas:** Se o aprendizado gerar uma regra geral, adicione-a na seção "Regras Rápidas" no topo.
- **Nunca delete:** Aprendizados antigos podem voltar a ser relevantes. Se uma lição se tornar obsoleta, marque-a como `[OBSOLETA — substituída por LL-XXX]`.

### Para a IA (em toda nova sessão)

Este arquivo **DEVE** ser lido como contexto antes de qualquer geração de código.

As seções são lidas nesta ordem de prioridade:
1. **Regras Rápidas** — lista enxuta de proibições e obrigações. Leia primeiro.
2. **Lições (LL-XXX)** — detalhamento de cada problema encontrado e como foi resolvido.
3. **Template** — use para registrar novos aprendizados.

---

## Regras Rápidas

1. **Rever `docs/` + `CLAUDE.md` antes de cada ação.** Antes de qualquer alteração, criação de arquivo, ou decisão técnica, reler toda a pasta `docs/` e o `CLAUDE.md`. Esta é a primeira ação em qualquer sessão ou tarefa.

---

## Lições

> Nenhuma lição registrada ainda. O projeto está na fase inicial (Spec 0010 pendente).

---

## Template

Use este template para registrar novas lições:

```
---

## LL-XXX: [Título descritivo]

**Data:** YYYY-MM-DD
**Contexto:** [O que estava a ser feito quando o problema surgiu]
**Problema:** [Descrição clara do problema encontrado]
**Causa:** [Causa raiz, se identificada]
**Solução:** [O que foi feito para resolver]
**Aprendizado:** [O que aprendemos com isso — regra geral, abordagem a evitar, etc.]
**Tags:** [área afetada, ex: auth, sql, react, jwt, cors, deploy]
```
