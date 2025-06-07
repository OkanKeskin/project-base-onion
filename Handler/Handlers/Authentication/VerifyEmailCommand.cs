using MediatR;
using System;

namespace Handler.Handlers.Authentication;

public class VerifyEmailCommand : IRequest<bool>
{
    public Guid AccountId { get; set; }
    public string Token { get; set; }
} 