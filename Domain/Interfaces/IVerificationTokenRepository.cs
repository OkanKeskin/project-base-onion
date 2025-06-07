using Domain.Entities;

namespace Domain.Interfaces;

public interface IVerificationTokenRepository : IRepository<VerificationToken>
{
    Task<VerificationToken> GetByTokenAsync(string token);
    Task<VerificationToken> GetLatestByAccountIdAsync(Guid accountId);
} 