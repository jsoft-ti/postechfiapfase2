namespace Application.Dto.Order;

public class UserOrderDto
{
    public string SortBy { get; set; } = "DisplayName";
    public bool Ascending { get; set; } = true;
}
