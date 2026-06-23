using MarcaAqui.Api.Models;
using Microsoft.Data.SqlClient;

namespace MarcaAqui.Api.Infrastructure;

public class AgendamentoRepository
{
    private readonly DbConnectionFactory _db;

    public AgendamentoRepository(DbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<Agendamento> CriarAsync(Agendamento agendamento)
    {
        using var connection = _db.CriarConexao();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Agendamentos (cliente_id, profissional_id, servico_id, data_hora_inicio, data_hora_fim, status)
            OUTPUT INSERTED.id
            VALUES (@cliente_id, @profissional_id, @servico_id, @data_hora_inicio, @data_hora_fim, @status)";

        cmd.Parameters.Add(new SqlParameter("@cliente_id", agendamento.ClienteId));
        cmd.Parameters.Add(new SqlParameter("@profissional_id", agendamento.ProfissionalId));
        cmd.Parameters.Add(new SqlParameter("@servico_id", agendamento.ServicoId));
        cmd.Parameters.Add(new SqlParameter("@data_hora_inicio", agendamento.DataHoraInicio));
        cmd.Parameters.Add(new SqlParameter("@data_hora_fim", agendamento.DataHoraFim));
        cmd.Parameters.Add(new SqlParameter("@status", agendamento.Status));

        var id = (int)await cmd.ExecuteScalarAsync();
        agendamento.Id = id;
        return agendamento;
    }

    public async Task<Agendamento?> ObterPorIdAsync(int id)
    {
        using var connection = _db.CriarConexao();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT id, cliente_id, profissional_id, servico_id, data_hora_inicio, data_hora_fim, status FROM Agendamentos WHERE id = @id";
        cmd.Parameters.Add(new SqlParameter("@id", id));

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
            return Mapear(reader);

        return null;
    }

    public async Task<List<Agendamento>> ObterPorClienteIdAsync(int clienteId)
    {
        using var connection = _db.CriarConexao();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT id, cliente_id, profissional_id, servico_id, data_hora_inicio, data_hora_fim, status
            FROM Agendamentos
            WHERE cliente_id = @cliente_id
            ORDER BY data_hora_inicio DESC";
        cmd.Parameters.Add(new SqlParameter("@cliente_id", clienteId));

        var agendamentos = new List<Agendamento>();
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            agendamentos.Add(Mapear(reader));

        return agendamentos;
    }

    public async Task<List<Agendamento>> ObterPorProfissionalEDataAsync(int profissionalId, DateTime data)
    {
        var inicioDia = data.Date;
        var fimDia = inicioDia.AddDays(1);

        using var connection = _db.CriarConexao();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            SELECT id, cliente_id, profissional_id, servico_id, data_hora_inicio, data_hora_fim, status
            FROM Agendamentos
            WHERE profissional_id = @profissional_id
              AND data_hora_inicio >= @inicio_dia
              AND data_hora_inicio < @fim_dia
              AND status != 'cancelado'
            ORDER BY data_hora_inicio";
        cmd.Parameters.Add(new SqlParameter("@profissional_id", profissionalId));
        cmd.Parameters.Add(new SqlParameter("@inicio_dia", inicioDia));
        cmd.Parameters.Add(new SqlParameter("@fim_dia", fimDia));

        var agendamentos = new List<Agendamento>();
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            agendamentos.Add(Mapear(reader));

        return agendamentos;
    }

    public async Task<bool> ExisteConflitoAsync(int profissionalId, DateTime inicio, DateTime fim, int? ignorarAgendamentoId = null)
    {
        using var connection = _db.CriarConexao();
        using var cmd = connection.CreateCommand();

        var sql = @"
            SELECT COUNT(1)
            FROM Agendamentos
            WHERE profissional_id = @profissional_id
              AND status != 'cancelado'
              AND data_hora_inicio < @fim
              AND data_hora_fim > @inicio";

        if (ignorarAgendamentoId.HasValue)
            sql += " AND id != @ignorar_id";

        cmd.CommandText = sql;
        cmd.Parameters.Add(new SqlParameter("@profissional_id", profissionalId));
        cmd.Parameters.Add(new SqlParameter("@inicio", inicio));
        cmd.Parameters.Add(new SqlParameter("@fim", fim));

        if (ignorarAgendamentoId.HasValue)
            cmd.Parameters.Add(new SqlParameter("@ignorar_id", ignorarAgendamentoId.Value));

        return (int)await cmd.ExecuteScalarAsync() > 0;
    }

    public async Task AtualizarStatusAsync(int id, string status)
    {
        using var connection = _db.CriarConexao();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "UPDATE Agendamentos SET status = @status WHERE id = @id";
        cmd.Parameters.Add(new SqlParameter("@id", id));
        cmd.Parameters.Add(new SqlParameter("@status", status));

        await cmd.ExecuteNonQueryAsync();
    }

    private static Agendamento Mapear(SqlDataReader reader)
    {
        var idxId = reader.GetOrdinal("id");
        var idxClienteId = reader.GetOrdinal("cliente_id");
        var idxProfissionalId = reader.GetOrdinal("profissional_id");
        var idxServicoId = reader.GetOrdinal("servico_id");
        var idxDataHoraInicio = reader.GetOrdinal("data_hora_inicio");
        var idxDataHoraFim = reader.GetOrdinal("data_hora_fim");
        var idxStatus = reader.GetOrdinal("status");

        return new Agendamento
        {
            Id = reader.GetInt32(idxId),
            ClienteId = reader.GetInt32(idxClienteId),
            ProfissionalId = reader.GetInt32(idxProfissionalId),
            ServicoId = reader.GetInt32(idxServicoId),
            DataHoraInicio = reader.GetDateTime(idxDataHoraInicio),
            DataHoraFim = reader.GetDateTime(idxDataHoraFim),
            Status = reader.GetString(idxStatus)
        };
    }
}
