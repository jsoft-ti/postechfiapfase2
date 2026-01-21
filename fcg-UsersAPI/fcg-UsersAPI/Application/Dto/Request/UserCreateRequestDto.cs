using Application.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Application.Dto.Request;

public class UserCreateRequestDto : ICreateUser
{
    [Required(ErrorMessage = "First Name é obrigatório")]
    [StringLength(50, MinimumLength = 2)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last Name é obrigatório")]
    [StringLength(50, MinimumLength = 2)]
    public string LastName { get; set; } = string.Empty;

    public string FullName => $"{FirstName} {LastName}".Trim();

    [Required(ErrorMessage = "Display Name é obrigatório")]
    [StringLength(30, MinimumLength = 3)]
    public string DisplayName { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [Range(typeof(DateTime), "1/1/1900", "1/1/2020")]
    public DateTime Birthday { get; set; }

    [EmailAddress]
    [Required]
    public string Email { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    public required string Password { get; set; }

    [Compare("Password", ErrorMessage = "Senhas não conferem")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
