using Domain.Entities;

namespace Domain.Interfaces;

public interface IResetPasswordTokenRepository : IRepository<ResetPasswordToken>
{
    Task<ResetPasswordToken> GetByTokenAsync(string token);
    Task<ResetPasswordToken> GetLatestByAccountIdAsync(Guid accountId);
} 