namespace Domain.Dtos.Authentication;

public class ResetPasswordRequest
{
    public string Token { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
} 