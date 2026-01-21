namespace Application.Dto.Response;

public class PurchaseResponseDto
{
    public Guid PurchaseId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public string GameName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime PurchaseDate { get; set; }
}