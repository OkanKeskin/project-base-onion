using System.Security.Cryptography;
using System.Text;

namespace Business.Services;

public interface IPasswordHashService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hashedPassword);
}

public class PasswordHashService : IPasswordHashService
{
    private const int SaltSize = 16; // 128 bit
    private const int KeySize = 64; // 512 bit
    private const int Iterations = 10000;
    
    public string HashPassword(string password)
    {
        using var algorithm = new Rfc2898DeriveBytes(
            password,
            SaltSize,
            Iterations,
            HashAlgorithmName.SHA512);
        
        var key = Convert.ToBase64String(algorithm.GetBytes(KeySize));
        var salt = Convert.ToBase64String(algorithm.Salt);

        return $"{salt}.{key}";
    }
    
    public bool VerifyPassword(string password, string hashedPassword)
    {
        var parts = hashedPassword.Split('.', 2);
        
        if (parts.Length != 2)
        {
            throw new FormatException("Unexpected hash format");
        }
        
        var salt = Convert.FromBase64String(parts[0]);
        var key = Convert.FromBase64String(parts[1]);

        using var algorithm = new Rfc2898DeriveBytes(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA512);
        
        var keyToCheck = algorithm.GetBytes(KeySize);
        
        return keyToCheck.SequenceEqual(key);
    }
} 