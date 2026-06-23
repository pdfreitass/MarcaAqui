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
2. **Atualizar status da spec ao iniciar e concluir.** Assim que uma spec começar a ser implementada, atualizar o `roadmap.md` e o arquivo da spec para 🟡 (em progresso). Ao concluir e verificar, atualizar para 🟢 (concluída) e recalcular a % na barra de progresso.
3. **Registar dificuldades no `licoes_aprendidas.md`.** Sempre que um erro não-trivial for encontrado e resolvido, registar aqui com o template LL-XXX para que a IA não cometa o mesmo erro em sessões futuras.

---

## Lições

### LL-001: Microsoft.Data.SqlClient 7.x — `GetInt32("nome")` removido (apenas ordinal)

**Data:** 2026-06-23
**Contexto:** Implementação da Spec 0010 — repositórios com ADO.NET.
**Problema:** Ao compilar, os métodos `reader.GetInt32("nome_coluna")`, `reader.GetString("nome_coluna")` e `reader.GetDateTime("nome_coluna")` falharam com erro CS1503: "não é possível converter de 'string' para 'int'".
**Causa:** No Microsoft.Data.SqlClient 7.x, os métodos tipados (`GetInt32`, `GetString`, `GetDateTime`, `GetDecimal`) só aceitam **ordinal** (int), não nome de coluna (string). As sobrecargas com string foram removidas.
**Solução:** Em todos os métodos `Mapear()` dos repositórios, obter primeiro o ordinal com `reader.GetOrdinal("nome_coluna")` e depois passar o ordinal para o método tipado:
```csharp
var idxId = reader.GetOrdinal("id");
Id = reader.GetInt32(idxId);
```
**Aprendizado:** Sempre usar `GetOrdinal()` + ordinal nos repositórios ADO.NET com Microsoft.Data.SqlClient 7.x+. Nunca passar string diretamente para `GetInt32`/`GetString`/etc.
**Tags:** sql, ado.net, infrastructure

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
