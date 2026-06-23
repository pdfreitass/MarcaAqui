namespace MarcaAqui.Api.Models;

public class Servico
{
    public int Id { get; set; }
    public int BarbeariaId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int DuracaoMinutos { get; set; }
    public decimal Preco { get; set; }
}
