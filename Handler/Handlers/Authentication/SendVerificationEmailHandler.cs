using Domain.Dtos.Authentication;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;
using Business.Services;
using System.Net;

namespace Handler.Handlers.Authentication;

public class SendVerificationEmailHandler : IRequestHandler<SendVerificationEmailCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _config;

    public SendVerificationEmailHandler(IUnitOfWork unitOfWork, ITokenService tokenService, IEmailService emailService, IConfiguration config)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _emailService = emailService;
        _config = config;
    }

    public async Task<bool> Handle(SendVerificationEmailCommand request, CancellationToken cancellationToken)
    {
        var account = await _unitOfWork.Accounts.GetByEmailAsync(request.Email);
        
        if (account == null)
        {
            throw ApiException.NotFound("Bu e-posta adresi ile ilişkili hesap bulunamadı");
        }

        // Generate verification token
        var token = _tokenService.GenerateRandomToken();
        var tokenExpiry = DateTime.UtcNow.AddHours(24);
        
        // Save token to verification tokens table
        var verificationToken = new VerificationToken
        {
            Token = token,
            ExpiryDate = tokenExpiry,
            AccountId = account.Id,
            IsUsed = false
        };
        
        await _unitOfWork.VerificationTokens.AddAsync(verificationToken);
        await _unitOfWork.CompleteAsync();
        
        // Send email with token
        var verificationLink = $"{_config["Application:FrontendUrl"]}/verify-email?accountId={account.Id}&token={token}";
        await _emailService.SendEmailVerificationAsync(account.Email, verificationLink);
        
        return true;
    }
} 