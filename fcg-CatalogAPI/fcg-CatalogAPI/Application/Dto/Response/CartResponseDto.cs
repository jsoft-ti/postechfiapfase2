namespace Application.Dto.Response;

public class CartResponseDto
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public IEnumerable<CartItemResponseDto> Items { get; set; } = [];
    public decimal Total => Items.Sum(i => i.Price);
}