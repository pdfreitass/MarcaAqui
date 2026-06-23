using System.Data;
using MarcaAqui.Api.Models;
using Microsoft.Data.SqlClient;

namespace MarcaAqui.Api.Infrastructure;

public class UsuarioRepository
{
    private readonly DbConnectionFactory _db;

    public UsuarioRepository(DbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<Usuario> CriarAsync(Usuario usuario)
    {
        using var connection = _db.CriarConexao();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Usuarios (nome, email, senha_hash, tipo, criado_em)
            OUTPUT INSERTED.id
            VALUES (@nome, @email, @senha_hash, @tipo, @criado_em)";

        cmd.Parameters.Add(new SqlParameter("@nome", usuario.Nome));
        cmd.Parameters.Add(new SqlParameter("@email", usuario.Email));
        cmd.Parameters.Add(new SqlParameter("@senha_hash", usuario.SenhaHash));
        cmd.Parameters.Add(new SqlParameter("@tipo", usuario.Tipo));
        cmd.Parameters.Add(new SqlParameter("@criado_em", DateTime.UtcNow));

        var id = (int)await cmd.ExecuteScalarAsync();
        usuario.Id = id;
        return usuario;
    }

    public async Task<Usuario?> ObterPorIdAsync(int id)
    {
        using var connection = _db.CriarConexao();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT id, nome, email, senha_hash, tipo, criado_em FROM Usuarios WHERE id = @id";
        cmd.Parameters.Add(new SqlParameter("@id", id));

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
            return Mapear(reader);

        return null;
    }

    public async Task<Usuario?> ObterPorEmailAsync(string email)
    {
        using var connection = _db.CriarConexao();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT id, nome, email, senha_hash, tipo, criado_em FROM Usuarios WHERE email = @email";
        cmd.Parameters.Add(new SqlParameter("@email", email));

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
            return Mapear(reader);

        return null;
    }

    public async Task<bool> EmailExisteAsync(string email)
    {
        using var connection = _db.CriarConexao();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT COUNT(1) FROM Usuarios WHERE email = @email";
        cmd.Parameters.Add(new SqlParameter("@email", email));

        return (int)await cmd.ExecuteScalarAsync() > 0;
    }

    private static Usuario Mapear(SqlDataReader reader)
    {
        var idxId = reader.GetOrdinal("id");
        var idxNome = reader.GetOrdinal("nome");
        var idxEmail = reader.GetOrdinal("email");
        var idxSenhaHash = reader.GetOrdinal("senha_hash");
        var idxTipo = reader.GetOrdinal("tipo");
        var idxCriadoEm = reader.GetOrdinal("criado_em");

        return new Usuario
        {
            Id = reader.GetInt32(idxId),
            Nome = reader.GetString(idxNome),
            Email = reader.GetString(idxEmail),
            SenhaHash = reader.GetString(idxSenhaHash),
            Tipo = reader.GetString(idxTipo),
            CriadoEm = reader.GetDateTime(idxCriadoEm)
        };
    }
}
