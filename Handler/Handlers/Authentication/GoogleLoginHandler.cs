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

public class GoogleLoginHandler : IRequestHandler<GoogleLoginCommand, AuthenticationResponse>
{
    private readonly IGoogleAuthService _googleAuthService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IConfiguration _config;

    public GoogleLoginHandler(IGoogleAuthService googleAuthService, IUnitOfWork unitOfWork, IJwtService jwtService, IConfiguration config)
    {
        _googleAuthService = googleAuthService;
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _config = config;
    }

    public async Task<AuthenticationResponse> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        var validatedInfo = await _googleAuthService.ValidateGoogleTokenAsync(request.IdToken);

        if (validatedInfo == null)
        {
            throw ApiException.Unauthorized("Google token doğrulanamadı");
        }

        var account = await _unitOfWork.Accounts.GetByEmailAsync(validatedInfo.Email);

        // If new user, register
        if (account == null)
        {
            account = new Account
            {
                Email = validatedInfo.Email,
                Type = AccountType.Unknown,
                Provider = AccountProvider.Google,
                EmailVerification = VerificationStatus.Verified // Google emails are already verified
            };

            await _unitOfWork.Accounts.AddAsync(account);
            await _unitOfWork.CompleteAsync();
        }
        // If account exists with email provider, throw error
        else if (account.Provider == AccountProvider.Email)
        {
            throw ApiException.Conflict("Bu e-posta adresi ile klasik bir hesap mevcut. Lütfen e-posta ve şifre ile giriş yapın.");
        }

        // Generate tokens
        var accessToken = _jwtService.GenerateAccessToken(account);
        var refreshToken = _jwtService.GenerateRefreshToken();
        
        // Save refresh token in the database
        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshToken,
            ExpiryDate = DateTime.UtcNow.AddDays(int.Parse(_config["JwtSettings:RefreshTokenExpirationDays"])),
            AccountId = account.Id,
            IsRevoked = false,
            IsUsed = false
        };
        
        await _unitOfWork.RefreshTokens.AddAsync(refreshTokenEntity);
        await _unitOfWork.CompleteAsync();

        return new AuthenticationResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiration = DateTime.UtcNow.AddMinutes(int.Parse(_config["JwtSettings:AccessTokenExpirationMinutes"])),
            AccountId = account.Id,
            Email = account.Email,
            AccountType = account.Type.ToString(),
            Success = true
        };
    }
} 