using System.ComponentModel.DataAnnotations;

namespace MarcaAqui.Api.DTOs;

public class RegistroDto
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [MinLength(2, ErrorMessage = "O nome deve ter no mínimo 2 caracteres.")]
    [MaxLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O email é obrigatório.")]
    [EmailAddress(ErrorMessage = "Email inválido.")]
    [MaxLength(200, ErrorMessage = "O email deve ter no máximo 200 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatória.")]
    [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres.")]
    [MaxLength(100, ErrorMessage = "A senha deve ter no máximo 100 caracteres.")]
    public string Senha { get; set; } = string.Empty;

    [Required(ErrorMessage = "O tipo é obrigatório.")]
    [RegularExpression("^(cliente|barbeiro)$", ErrorMessage = "O tipo deve ser 'cliente' ou 'barbeiro'.")]
    public string Tipo { get; set; } = string.Empty;
}
