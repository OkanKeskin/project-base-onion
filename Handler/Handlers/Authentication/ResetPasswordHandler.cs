using Domain.Dtos.Authentication;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;
using Business.Services;
using System.Net;

namespace Handler.Handlers.Authentication;

public class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHashService _passwordHashService;

    public ResetPasswordHandler(IUnitOfWork unitOfWork, IPasswordHashService passwordHashService)
    {
        _unitOfWork = unitOfWork;
        _passwordHashService = passwordHashService;
    }

    public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var resetToken = await _unitOfWork.ResetPasswordTokens.GetByTokenAsync(request.Token);
        
        if (resetToken == null || resetToken.IsUsed)
        {
            throw ApiException.NotFound("Geçersiz veya süresi dolmuş sıfırlama bağlantısı");
        }

        if (resetToken.ExpiryDate < DateTime.UtcNow)
        {
            throw ApiException.BadRequest("Sıfırlama bağlantısının süresi dolmuş");
        }

        var account = await _unitOfWork.Accounts.GetAsync(resetToken.AccountId);
        
        if (account == null)
        {
            throw ApiException.NotFound("Hesap bulunamadı");
        }

        // Hash new password
        account.Password = _passwordHashService.HashPassword(request.Password);
        
        // Mark token as used
        resetToken.IsUsed = true;
        _unitOfWork.ResetPasswordTokens.Update(resetToken);
        _unitOfWork.Accounts.Update(account);
        
        await _unitOfWork.CompleteAsync();
        
        return true;
    }
} 