using MarcaAqui.Api.Models;
using Microsoft.Data.SqlClient;

namespace MarcaAqui.Api.Infrastructure;

public class ProfissionalRepository
{
    private readonly DbConnectionFactory _db;

    public ProfissionalRepository(DbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<Profissional> CriarAsync(Profissional profissional)
    {
        using var connection = _db.CriarConexao();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Profissionais (usuario_id, barbearia_id)
            OUTPUT INSERTED.id
            VALUES (@usuario_id, @barbearia_id)";

        cmd.Parameters.Add(new SqlParameter("@usuario_id", profissional.UsuarioId));
        cmd.Parameters.Add(new SqlParameter("@barbearia_id", profissional.BarbeariaId));

        var id = (int)await cmd.ExecuteScalarAsync();
        profissional.Id = id;
        return profissional;
    }

    public async Task<Profissional?> ObterPorUsuarioIdAsync(int usuarioId)
    {
        using var connection = _db.CriarConexao();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT id, usuario_id, barbearia_id FROM Profissionais WHERE usuario_id = @usuario_id";
        cmd.Parameters.Add(new SqlParameter("@usuario_id", usuarioId));

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
            return Mapear(reader);

        return null;
    }

    public async Task<List<Profissional>> ObterPorBarbeariaIdAsync(int barbeariaId)
    {
        using var connection = _db.CriarConexao();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT id, usuario_id, barbearia_id FROM Profissionais WHERE barbearia_id = @barbearia_id";
        cmd.Parameters.Add(new SqlParameter("@barbearia_id", barbeariaId));

        var profissionais = new List<Profissional>();
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            profissionais.Add(Mapear(reader));

        return profissionais;
    }

    private static Profissional Mapear(SqlDataReader reader)
    {
        var idxId = reader.GetOrdinal("id");
        var idxUsuarioId = reader.GetOrdinal("usuario_id");
        var idxBarbeariaId = reader.GetOrdinal("barbearia_id");

        return new Profissional
        {
            Id = reader.GetInt32(idxId),
            UsuarioId = reader.GetInt32(idxUsuarioId),
            BarbeariaId = reader.GetInt32(idxBarbeariaId)
        };
    }
}
