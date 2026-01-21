namespace Application.Dto.Response;

public class PlayerCreateDto
{
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
}