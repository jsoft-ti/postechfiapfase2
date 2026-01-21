namespace Application.Dto.Request;

public class OrderPlacedEventRequestDto
{
    public Guid? UserId { get; set; }
    public Guid? GameId { get; set; }
    public decimal? Price { get; set; }
   
}