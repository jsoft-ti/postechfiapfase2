namespace Application.Dto.Response;

public class GalleryGameResponseDto : GameResponseDto
{
    public decimal Price { get; set; }
    public decimal FinalPrice { get; set; }
    public string PromotionDescription { get; set; } = string.Empty;
    public bool OnSale { get; set; }
    public string TypePromotion { get; set; } = string.Empty;
    public decimal PromotionValue { get; set; }
}