using Domain.Dtos.Authentication;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;
using Business.Services;
using System.Net;

namespace Handler.Handlers.Authentication;

public class ForgotPasswordHandler : IRequestHandler<ForgotPasswordCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _config;

    public ForgotPasswordHandler(IUnitOfWork unitOfWork, ITokenService tokenService, IEmailService emailService, IConfiguration config)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _emailService = emailService;
        _config = config;
    }

    public async Task<bool> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var account = await _unitOfWork.Accounts.GetByEmailAsync(request.Email);
        
        if (account == null)
        {
            throw ApiException.NotFound("Bu e-posta adresi ile ilişkili hesap bulunamadı");
        }

        // Generate password reset token
        var token = _tokenService.GenerateRandomToken();
        var tokenExpiry = DateTime.UtcNow.AddHours(24);
        
        // Save token to reset password tokens table
        var resetToken = new ResetPasswordToken
        {
            Token = token,
            ExpiryDate = tokenExpiry,
            AccountId = account.Id,
            IsUsed = false
        };
        
        await _unitOfWork.ResetPasswordTokens.AddAsync(resetToken);
        await _unitOfWork.CompleteAsync();
        
        // Send email with token
        var resetLink = $"{_config["Application:FrontendUrl"]}/reset-password?token={token}";
        await _emailService.SendPasswordResetEmailAsync(account.Email, resetLink);
        
        return true;
    }
} 