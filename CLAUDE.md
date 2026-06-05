# CLAUDE.md - Regras do Projeto MarcaAqui

## Princípios de Código

- **KISS em primeiro lugar.** Soluções simples e diretas sempre têm prioridade.
- **DRY sempre que possível.** Evita repetição de código sempre que a abstração for simples.
- **KISS > DRY.** Se para não repetir código fores obrigado a criar algo muito complexo, prefere a repetição. Exemplo: dois casos de `switch` simples e repetidos são melhores do que uma abstração genérica difícil de entender.

## Comunicação com IA

Este projeto é 100% codificado por IA autônoma. Escreve de forma simples e direta para que uma IA consiga interpretar sem ambiguidade. Evita jargões desnecessários e estruturas excessivamente complexas.

## Repositório GitHub

- **NÃO faças commits, pushes, pulls, merges ou qualquer operação git remota.**
- **NÃO interajas com o GitHub** a não ser que o utilizador (Castor) peça explicitamente.
- Operações locais de git (`git status`, `git diff`, etc.) são permitidas para consulta, mas nunca commits.

## Regras de Implementação

- **NÃO implementes código sem um documento de spec aprovado pelo utilizador.**
- Cada spec tem de ser aprovada pelo Castor antes de qualquer implementação começar.
- **Na dúvida, NÃO assumes. Pergunta ao Castor.** Qualquer ambiguidade, decisão de design, ou caminho incerto deve ser esclarecido com o utilizador antes de agir.

## Numeração de Specs

- As specs principais são numeradas de 10 em 10: `0010`, `0020`, `0030`, etc.
- Correções de bugs e hotfixes são associados à spec original acrescentando 1:
  - Spec `0010` → bug fix: `0011`
  - Spec `0020` → hotfix: `0021`, outro hotfix: `0022`
  - O número base (dezena) nunca muda, apenas incrementa a unidade.

## Assinatura

- O utilizador é **Castor**.
