using Domain.Interfaces;
using System.Security.Cryptography;

namespace Business.Services;

public class TokenService : ITokenService
{
    public string GenerateRandomToken()
    {
        // Generate a cryptographically secure random token
        using var rng = RandomNumberGenerator.Create();
        var tokenBytes = new byte[32]; // 256 bits
        rng.GetBytes(tokenBytes);
        return Convert.ToBase64String(tokenBytes);
    }

    public bool ValidateToken(string token)
    {
        // For simple token validation, just check if it's not empty and is a valid Base64 string
        if (string.IsNullOrEmpty(token))
            return false;

        try
        {
            // Try to decode the token to validate it's a proper Base64 string
            Convert.FromBase64String(token);
            return true;
        }
        catch
        {
            return false;
        }
    }
} 