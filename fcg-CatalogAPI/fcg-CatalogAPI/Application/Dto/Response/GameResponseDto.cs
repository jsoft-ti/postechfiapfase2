namespace Application.Dto.Response;

public class GameResponseDto
{
    public Guid Id { get; set; }
    public string EAN { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string SubTitle { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    public decimal? Price { get; set; }
}