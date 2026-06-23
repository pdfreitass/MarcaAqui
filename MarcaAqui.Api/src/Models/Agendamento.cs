namespace MarcaAqui.Api.Models;

public class Agendamento
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public int ProfissionalId { get; set; }
    public int ServicoId { get; set; }
    public DateTime DataHoraInicio { get; set; }
    public DateTime DataHoraFim { get; set; }
    public string Status { get; set; } = "confirmado";  // "confirmado", "cancelado", "concluido"
}
