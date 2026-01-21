namespace Application.Dto.Response;

public class PlayerResponseDto
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public int GamesCount { get; set; }
}