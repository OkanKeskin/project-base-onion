using MediatR;

namespace Handler.Handlers.Authentication;

public class SendVerificationEmailCommand : IRequest<bool>
{
    public string Email { get; set; }
} 