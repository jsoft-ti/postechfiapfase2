using System.ComponentModel.DataAnnotations;

namespace Application.Dto.Request;

public class GameCreateRequestDto
{
    [Display(Name = "EAN")]
    [Required(ErrorMessage = "{0} do jogo é obrigatório")]
    [StringLength(13, MinimumLength = 13, ErrorMessage = "O {0} deve ter exatamente {1} caracteres")]
    [RegularExpression(@"^\d{13}$", ErrorMessage = "{0} deve conter exatamente 13 dígitos numéricos.")]
    public string EAN { get; set; } = string.Empty;

    [Display(Name = "Título")]
    [Required(ErrorMessage = "{0} é obrigatório")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O {0} deve ter entre {2} e {1} caracteres")]
    public string Title { get; set; } = string.Empty;

    [StringLength(50, MinimumLength = 1, ErrorMessage = "O subtítulo deve ter entre {2} e {1} caracteres.")]
    public string? SubTitle { get; set; }

    [Display(Name = "Gênero")]
    [Required(ErrorMessage = "{0} é obrigatório")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "O {0} deve ter entre {2} e {1} caracteres")]
    public string Genre { get; set; } = string.Empty;

    [Display(Name = "Descrição")]
    [StringLength(500, ErrorMessage = "A {0} não pode exceder {1} caracteres")]
    public string? Description { get; set; }

    [Display(Name = "Preço")]
    [Required(ErrorMessage = "{0} é obrigatório")]
    [Range(0.00, 9999.99, ErrorMessage = "O {0} deve estar entre R$ 0,00 (grátis) e R$ 9.999,99")]
    [DataType(DataType.Currency)]
    public decimal Price { get; set; }
}