namespace Application.Dto.Filter;

public class GameFilterDto
{
    public string? Name { get; set; }
    public string? Genre { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
}