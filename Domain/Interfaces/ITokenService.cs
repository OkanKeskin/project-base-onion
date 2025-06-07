namespace Domain.Interfaces;

public interface ITokenService
{
    string GenerateRandomToken();
    bool ValidateToken(string token);
} 