namespace Application.Dto.Response;

public class UserAuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserInfoResponseDto User { get; set; } = new();
}
