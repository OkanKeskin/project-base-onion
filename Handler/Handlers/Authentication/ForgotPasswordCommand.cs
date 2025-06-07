using MediatR;

namespace Handler.Handlers.Authentication;

public class ForgotPasswordCommand : IRequest<bool>
{
    public string Email { get; set; }
} 