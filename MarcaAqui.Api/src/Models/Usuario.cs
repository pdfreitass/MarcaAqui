namespace MarcaAqui.Api.Models;

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;  // "cliente" ou "barbeiro"
    public DateTime CriadoEm { get; set; }
}
