namespace Application.Dto.Request;

public class PurchaseCreateRequestDto
{
    public Guid PlayerId { get; set; }
    public Guid GameId { get; set; }
}
