namespace MarcaAqui.Api.DTOs;

public class TokenDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiraEm { get; set; }
    public string Tipo { get; set; } = string.Empty;
}
