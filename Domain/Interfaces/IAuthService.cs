using Domain.Entities;

namespace Domain.Interfaces;

public interface IAuthService
{
    Task<(string AccessToken, string RefreshToken)> GenerateTokensAsync(Account account);
    Task<string> GenerateAccessTokenAsync(Account account);
    Task<string> GenerateRefreshTokenAsync();
    Task<bool> ValidateTokenAsync(string token);
    Task<(string AccessToken, string RefreshToken)> RefreshTokenAsync(string refreshToken);
    Task RevokeRefreshTokenAsync(string refreshToken);
    Guid? GetUserIdFromToken(string token);
} 