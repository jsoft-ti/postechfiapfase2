namespace Application.Dto.Response;

public class OrderPlacedEventResponseDto
{
    public Guid? UserId { get; set; }
    public Guid? GameId { get; set; }
    public decimal? Price { get; set; }
   
}