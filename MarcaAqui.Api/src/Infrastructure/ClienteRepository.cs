using MarcaAqui.Api.Models;
using Microsoft.Data.SqlClient;

namespace MarcaAqui.Api.Infrastructure;

public class ClienteRepository
{
    private readonly DbConnectionFactory _db;

    public ClienteRepository(DbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<Cliente> CriarAsync(Cliente cliente)
    {
        using var connection = _db.CriarConexao();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Clientes (usuario_id, telefone)
            OUTPUT INSERTED.id
            VALUES (@usuario_id, @telefone)";

        cmd.Parameters.Add(new SqlParameter("@usuario_id", cliente.UsuarioId));
        cmd.Parameters.Add(new SqlParameter("@telefone", (object?)cliente.Telefone ?? DBNull.Value));

        var id = (int)await cmd.ExecuteScalarAsync();
        cliente.Id = id;
        return cliente;
    }

    public async Task<Cliente?> ObterPorUsuarioIdAsync(int usuarioId)
    {
        using var connection = _db.CriarConexao();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT id, usuario_id, telefone FROM Clientes WHERE usuario_id = @usuario_id";
        cmd.Parameters.Add(new SqlParameter("@usuario_id", usuarioId));

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
            return Mapear(reader);

        return null;
    }

    private static Cliente Mapear(SqlDataReader reader)
    {
        var idxId = reader.GetOrdinal("id");
        var idxUsuarioId = reader.GetOrdinal("usuario_id");
        var idxTelefone = reader.GetOrdinal("telefone");

        return new Cliente
        {
            Id = reader.GetInt32(idxId),
            UsuarioId = reader.GetInt32(idxUsuarioId),
            Telefone = reader.IsDBNull(idxTelefone) ? null : reader.GetString(idxTelefone)
        };
    }
}
