using Domain.Entities;

namespace Domain.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(Account account);
    string GenerateRefreshToken();
    bool ValidateToken(string token);
    Task<bool> ValidateTokenAsync(string token);
} 