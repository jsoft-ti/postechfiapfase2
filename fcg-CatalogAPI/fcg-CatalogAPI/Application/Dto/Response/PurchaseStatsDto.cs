namespace Application.Dto.Response;

public class PurchaseStatsDto
{
    public int TotalPurchases { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal PeriodRevenue { get; set; }
    public string Period { get; set; } = string.Empty;
}