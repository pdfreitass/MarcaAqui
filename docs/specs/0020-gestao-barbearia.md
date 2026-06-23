# Spec 0020 — Cadastro e Gestão da Barbearia

**Status:** 🔴 Por iniciar
**Data de início:** —
**Data de conclusão:** —
**Dependências:** [Spec 0010 — Fundação](0010-fundacao.md)

> 📖 Esta spec implementa a gestão da barbearia descrita em [`docs/visao.md`](../visao.md) — secção "Lado do Barbeiro". O barbeiro gere a sua barbearia, serviços e equipa.

---

## Objetivo

Barbeiro autenticado consegue criar e gerir a sua barbearia, cadastrar os serviços que oferece e adicionar/remover profissionais da sua equipa. Isolamento multi-tenant: cada barbeiro só acede à sua própria barbearia.

---

## Escopo

### Backend — Infrastructure (novos repositórios)

| Classe | Métodos |
|---|---|
| `BarbeariaRepository.cs` | `Criar(Barbearia)`, `ObterPorId(int)`, `ObterPorDonoId(int)`, `Atualizar(Barbearia)` |

### Backend — Service (novos serviços)

| Classe | Responsabilidade |
|---|---|
| `BarbeariaService.cs` | `CriarBarbearia(dto, usuarioId)` — um barbeiro só pode ter uma barbearia. `ObterMinhaBarbearia(usuarioId)` — retorna a barbearia do barbeiro autenticado. `AtualizarBarbearia(dto, usuarioId)` — edita nome, endereço, telefone. |
| `ServicoService.cs` | `CriarServico(dto, barbeariaId)`, `ListarServicos(barbeariaId)`, `AtualizarServico(id, dto, barbeariaId)`, `RemoverServico(id, barbeariaId)`. |
| `ProfissionalService.cs` | `AdicionarProfissional(dto, barbeariaId)` — vincula um utilizador existente (tipo barbeiro) à barbearia. `ListarProfissionais(barbeariaId)`, `RemoverProfissional(id, barbeariaId)`. |

### Backend — Controllers

| Endpoint | Auth | Descrição |
|---|---|---|
| `POST /barbearias` | Barbeiro | Criar barbearia. Retorna 201. |
| `GET /barbearias/minha` | Barbeiro | Obter barbearia do barbeiro logado. |
| `PUT /barbearias/{id}` | Barbeiro | Atualizar dados da barbearia. |
| `POST /barbearias/{id}/servicos` | Barbeiro | Criar serviço. Retorna 201. |
| `GET /barbearias/{id}/servicos` | Público | Listar serviços da barbearia. Sem auth. |
| `PUT /barbearias/{id}/servicos/{servicoId}` | Barbeiro | Atualizar serviço. |
| `DELETE /barbearias/{id}/servicos/{servicoId}` | Barbeiro | Remover serviço. |
| `POST /barbearias/{id}/profissionais` | Barbeiro | Adicionar profissional à equipa. |
| `GET /barbearias/{id}/profissionais` | Barbeiro | Listar profissionais da barbearia. |
| `DELETE /barbearias/{id}/profissionais/{profissionalId}` | Barbeiro | Remover profissional da equipa. |

### DTOs envolvidos

| DTO | Campos |
|---|---|
| `BarbeariaDto.cs` | `Id`, `Nome`, `Endereco`, `Telefone`, `UsuarioDonoId` |
| `CriarBarbeariaDto.cs` | `Nome`, `Endereco`, `Telefone` |
| `ServicoDto.cs` | `Id`, `Nome`, `DuracaoMinutos`, `Preco`, `BarbeariaId` |
| `CriarServicoDto.cs` | `Nome`, `DuracaoMinutos`, `Preco` |
| `ProfissionalDto.cs` | `Id`, `Nome`, `Email`, `Telefone`, `BarbeariaId` |
| `AdicionarProfissionalDto.cs` | `UsuarioId` |

---

## Regras de Negócio

1. **Um barbeiro = uma barbearia.** `POST /barbearias` retorna 409 se o barbeiro já tiver uma barbearia.
2. **Isolamento multi-tenant.** O `barbearia_id` é obtido do token JWT (claim), nunca do input do utilizador.
3. **Serviços pertencem à barbearia.** Ao criar/editar/remover, o `barbearia_id` do serviço é sempre o do barbeiro autenticado.
4. **Profissionais só podem ser adicionados se o utilizador-alvo existir e for do tipo `barbeiro`.**
5. **Barbeiro A não pode aceder a dados da barbearia B.** Todas as queries filtram por `barbearia_id`.

---

## Verificação

- [ ] Barbeiro autenticado cria a sua barbearia (201)
- [ ] Tentar criar segunda barbearia retorna 409
- [ ] Barbeiro edita dados da sua barbearia (200)
- [ ] Barbeiro adiciona serviços à sua barbearia (201)
- [ ] `GET /barbearias/{id}/servicos` público retorna serviços sem auth
- [ ] Barbeiro edita e remove serviços (200/204)
- [ ] Barbeiro adiciona profissional à equipa (201)
- [ ] Barbeiro lista e remove profissionais da equipa
- [ ] Barbeiro A não consegue aceder/alterar dados da barbearia do barbeiro B (403/404)
- [ ] Serviços removidos não afetam outras barbearias

---

## Subspecs

| Spec | Descrição | Status |
|---|---|---|
| — | Nenhuma subspec ainda | — |
