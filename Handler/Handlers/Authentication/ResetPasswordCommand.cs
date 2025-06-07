using MediatR;

namespace Handler.Handlers.Authentication;

public class ResetPasswordCommand : IRequest<bool>
{
    public string Token { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
} 