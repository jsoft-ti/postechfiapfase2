namespace Application.Dto.Response;

public class LibraryGameResponseDto : GameResponseDto
{
    public Guid PlayerId { get; set; }
    public string PlayerDisplayName { get; set; } = string.Empty;
    public DateTime PurchaseDate { get; set; }
    public decimal PurchasePrice { get; set; }
}