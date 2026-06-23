using MarcaAqui.Api.Models;
using Microsoft.Data.SqlClient;

namespace MarcaAqui.Api.Infrastructure;

public class ServicoRepository
{
    private readonly DbConnectionFactory _db;

    public ServicoRepository(DbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<Servico?> ObterPorIdAsync(int id)
    {
        using var connection = _db.CriarConexao();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT id, barbearia_id, nome, duracao_minutos, preco FROM Servicos WHERE id = @id";
        cmd.Parameters.Add(new SqlParameter("@id", id));

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
            return Mapear(reader);

        return null;
    }

    public async Task<List<Servico>> ObterPorBarbeariaIdAsync(int barbeariaId)
    {
        using var connection = _db.CriarConexao();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT id, barbearia_id, nome, duracao_minutos, preco FROM Servicos WHERE barbearia_id = @barbearia_id";
        cmd.Parameters.Add(new SqlParameter("@barbearia_id", barbeariaId));

        var servicos = new List<Servico>();
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            servicos.Add(Mapear(reader));

        return servicos;
    }

    private static Servico Mapear(SqlDataReader reader)
    {
        var idxId = reader.GetOrdinal("id");
        var idxBarbeariaId = reader.GetOrdinal("barbearia_id");
        var idxNome = reader.GetOrdinal("nome");
        var idxDuracaoMinutos = reader.GetOrdinal("duracao_minutos");
        var idxPreco = reader.GetOrdinal("preco");

        return new Servico
        {
            Id = reader.GetInt32(idxId),
            BarbeariaId = reader.GetInt32(idxBarbeariaId),
            Nome = reader.GetString(idxNome),
            DuracaoMinutos = reader.GetInt32(idxDuracaoMinutos),
            Preco = reader.GetDecimal(idxPreco)
        };
    }
}
