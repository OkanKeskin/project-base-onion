namespace Domain.Dtos.Authentication;

public class AuthenticationResponse
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime AccessTokenExpiration { get; set; }
    public Guid AccountId { get; set; }
    public string Email { get; set; }
    public string AccountType { get; set; }
    public bool Success { get; set; }
} 