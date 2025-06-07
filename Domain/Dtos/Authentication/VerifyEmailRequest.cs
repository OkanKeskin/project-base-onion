namespace Domain.Dtos.Authentication;

public class VerifyEmailRequest
{
    public Guid AccountId { get; set; }
    public string Token { get; set; }
} 