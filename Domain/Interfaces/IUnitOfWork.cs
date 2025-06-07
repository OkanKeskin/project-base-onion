using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Domain.Interfaces;

public interface IUnitOfWork
{
    IAccountRepository Accounts { get; }
    IMemberRepository Members { get; }
    IOwnerRepository Owners { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    IVerificationTokenRepository VerificationTokens { get; }
    IResetPasswordTokenRepository ResetPasswordTokens { get; }
    
    ChangeTracker ChangeTracker { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));
    Task<int> CompleteAsync();
}