namespace Application.Dto.Response;

public class CartItemResponseDto
{
    public Guid GameId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string SubTitle { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
