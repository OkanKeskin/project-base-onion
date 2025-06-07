using Domain.Dtos.Authentication;
using MediatR;

namespace Handler.Handlers.Authentication;

public class GoogleLoginCommand : IRequest<AuthenticationResponse>
{
    public string IdToken { get; set; }
} 