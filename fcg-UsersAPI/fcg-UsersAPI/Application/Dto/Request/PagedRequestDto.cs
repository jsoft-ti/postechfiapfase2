using System.ComponentModel.DataAnnotations;

namespace Application.Dto.Request;

public class PagedRequestDto<TFilter, TOrder>
{
    [Display(Name = "Página")]
    [Range(1, int.MaxValue, ErrorMessage = "O número da {0} deve ser maior que 0.")]
    public int PageNumber { get; set; } = 1;

    [Display(Name = "Itens por página")]
    [Range(1, 100, ErrorMessage = "O {0} deve estar entre {1} e {2}.")]
    public int PageSize { get; set; } = 10;

    public TOrder? OrderBy { get; set; } // Objeto genérico para ordenação
    public TFilter? Filter { get; set; } // Objeto genérico para filtros
}
