using System.Data;
using Microsoft.Data.SqlClient;

namespace MarcaAqui.Api.Infrastructure;

public class DbConnectionFactory
{
    private readonly string _connectionString;
    private readonly string _scriptsPath;

    public DbConnectionFactory(IConfiguration configuration, IWebHostEnvironment env)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionString 'DefaultConnection' não configurada.");

        // Caminho da pasta Database (funciona em dev e publicado)
        _scriptsPath = Path.Combine(env.ContentRootPath, "src", "Database");
        if (!Directory.Exists(_scriptsPath))
            _scriptsPath = Path.Combine(env.ContentRootPath, "Database");
    }

    public SqlConnection CriarConexao()
    {
        var connection = new SqlConnection(_connectionString);
        connection.Open();
        return connection;
    }

    public async Task AplicarMigrationsAsync()
    {
        if (!Directory.Exists(_scriptsPath))
            return;

        var arquivosSql = Directory.GetFiles(_scriptsPath, "*.sql")
            .OrderBy(f => Path.GetFileName(f))
            .ToList();

        if (arquivosSql.Count == 0)
            return;

        using var connection = CriarConexao();

        // Garante que a tabela _Migration existe (script 000)
        foreach (var arquivo in arquivosSql)
        {
            var nomeArquivo = Path.GetFileName(arquivo);
            var script = await File.ReadAllTextAsync(arquivo);

            // Extrai número do nome do arquivo (ex: "001_CriarTabelaUsuarios.sql" → 1)
            var numeroStr = nomeArquivo.Split('_')[0].TrimStart('0');
            if (!int.TryParse(numeroStr, out var numero))
                continue;

            // Verifica se já foi aplicado
            using var cmdCheck = connection.CreateCommand();
            cmdCheck.CommandText = "SELECT COUNT(1) FROM _Migration WHERE Numero = @numero";
            cmdCheck.Parameters.Add(new SqlParameter("@numero", numero));

            var jaAplicado = (int)cmdCheck.ExecuteScalar()! > 0;
            if (jaAplicado)
                continue;

            // Executa o script
            using var cmd = connection.CreateCommand();
            cmd.CommandText = script;
            cmd.CommandType = CommandType.Text;
            await cmd.ExecuteNonQueryAsync();

            // Regista na tabela _Migration
            using var cmdRegistro = connection.CreateCommand();
            cmdRegistro.CommandText = "INSERT INTO _Migration (Numero, Nome) VALUES (@numero, @nome)";
            cmdRegistro.Parameters.Add(new SqlParameter("@numero", numero));
            cmdRegistro.Parameters.Add(new SqlParameter("@nome", nomeArquivo));
            await cmdRegistro.ExecuteNonQueryAsync();
        }
    }
}
