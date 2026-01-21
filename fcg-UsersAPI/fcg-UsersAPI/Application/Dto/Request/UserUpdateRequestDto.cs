using System.ComponentModel.DataAnnotations;

namespace Application.Dto.Request;

public class UserUpdateRequestDto
{
    [Required(ErrorMessage = "First Name é obrigatório")]
    [StringLength(50, MinimumLength = 2)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last Name é obrigatório")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Display Name é obrigatório")]
    [StringLength(30, MinimumLength = 3)]
    public string DisplayName { get; set; } = string.Empty;

    [EmailAddress]
    [Required]
    public string Email { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [Range(typeof(DateTime), "1/1/1900", "1/1/2020")]
    public DateTime Birthday { get; set; }

    [DataType(DataType.Password)]
    public required string Password { get; set; }

    [DataType(DataType.Password)]
    public required string NewPassword { get; set; }

    [Compare("NewPassword", ErrorMessage = "Senhas não conferem Nova senha confimação da nova senha")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}