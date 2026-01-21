namespace Application.Dto.Order;

public class GameOrderDto
{
    public string? OrderBy { get; set; } // Ex.: "Name", "Price"
    public bool Ascending { get; set; } = true;
}
