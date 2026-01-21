namespace Application.Dto.Order;

public class PurchaseOrderDto
{
    public string SortBy { get; set; } = "date";
    public bool Ascending { get; set; } = false;
}
