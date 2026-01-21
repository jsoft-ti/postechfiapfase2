using System.ComponentModel.DataAnnotations;

namespace Application.Dto.Request;

public class GalleryGameCreateRequestDto : GameCreateRequestDto
{
    public string? PromotionType { get; set; }
    public decimal? PromotionValue { get; set; }
    public DateTime? PromotionStartDate { get; set; }
    public DateTime? PromotionEndDate { get; set; }
}