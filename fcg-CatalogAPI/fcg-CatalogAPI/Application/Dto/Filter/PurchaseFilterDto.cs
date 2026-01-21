namespace Application.Dto.Filter;

public class PurchaseFilterDto
{
    public string? PlayerName { get; set; }
    public string? GameName { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
