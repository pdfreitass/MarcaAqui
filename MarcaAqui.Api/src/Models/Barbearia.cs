namespace MarcaAqui.Api.Models;

public class Barbearia
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Endereco { get; set; }
    public string? Telefone { get; set; }
    public int UsuarioDonoId { get; set; }
}
