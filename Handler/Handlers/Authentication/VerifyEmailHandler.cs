using Domain.Dtos.Authentication;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;
using Business.Services;
using System.Net;

namespace Handler.Handlers.Authentication;

public class VerifyEmailHandler : IRequestHandler<VerifyEmailCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public VerifyEmailHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var account = await _unitOfWork.Accounts.GetByIdAsync(request.AccountId);
        
        if (account == null)
        {
            throw ApiException.NotFound("Hesap bulunamadı");
        }

        var verificationToken = await _unitOfWork.VerificationTokens.GetByTokenAsync(request.Token);
        
        if (verificationToken == null || verificationToken.IsUsed || verificationToken.AccountId != account.Id)
        {
            throw ApiException.BadRequest("Geçersiz doğrulama kodu");
        }

        if (verificationToken.ExpiryDate < DateTime.UtcNow)
        {
            throw ApiException.BadRequest("Doğrulama kodunun süresi dolmuş");
        }

        account.EmailVerification = VerificationStatus.Verified;
        
        // Mark token as used
        verificationToken.IsUsed = true;
        _unitOfWork.VerificationTokens.Update(verificationToken);
        _unitOfWork.Accounts.Update(account);

        await _unitOfWork.CompleteAsync();

        return true;
    }
} 