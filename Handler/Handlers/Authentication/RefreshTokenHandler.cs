using Domain.Dtos.Authentication;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;
using Business.Services;
using System.Net;

namespace Handler.Handlers.Authentication;

public class RefreshTokenCommand : IRequest<AuthenticationResponse>
{
    public RefreshTokenRequest Request { get; set; }
}

public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, AuthenticationResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IConfiguration _configuration;

    public RefreshTokenHandler(
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _configuration = configuration;
    }

    public async Task<AuthenticationResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Find the refresh token in the database
        var refreshToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(request.Request.RefreshToken);
        
        // Validate the token
        if (refreshToken == null || refreshToken.IsUsed || refreshToken.IsRevoked || 
            refreshToken.ExpiryDate < DateTime.UtcNow)
        {
            throw ApiException.Unauthorized("Invalid or expired refresh token");
        }
        
        // Get the account associated with the token
        var account = await _unitOfWork.Accounts.GetAsync(refreshToken.AccountId);
        
        if (account == null)
        {
            throw ApiException.NotFound("Account not found");
        }

        // Mark the current token as used
        refreshToken.IsUsed = true;
        _unitOfWork.RefreshTokens.Update(refreshToken);
        
        // Generate new tokens
        var accessToken = _jwtService.GenerateAccessToken(account);
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        // Store new refresh token
        var newRefreshTokenEntity = new RefreshToken
        {
            Token = newRefreshToken,
            ExpiryDate = DateTime.UtcNow.AddDays(int.Parse(_configuration["JwtSettings:RefreshTokenExpirationDays"])),
            AccountId = account.Id,
            IsRevoked = false,
            IsUsed = false
        };
        
        await _unitOfWork.RefreshTokens.AddAsync(newRefreshTokenEntity);
        await _unitOfWork.CompleteAsync();

        return new AuthenticationResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            AccessTokenExpiration = DateTime.UtcNow.AddMinutes(
                int.Parse(_configuration["JwtSettings:AccessTokenExpirationMinutes"])),
            AccountId = account.Id,
            Email = account.Email,
            AccountType = account.Type.ToString(),
            Success = true
        };
    }
} 