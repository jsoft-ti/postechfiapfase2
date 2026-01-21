using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Application.Dto.Response;

public class UserLoginRequestDto
{
    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Senha é obrigatória")]
    [PasswordPropertyText]
    public string Password { get; set; } = string.Empty;
}
