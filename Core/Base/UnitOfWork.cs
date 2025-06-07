using Core.Contexts;
using Core.Repositories;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Serilog;

namespace Core.Base;

public class UnitOfWork: IUnitOfWork
{
    private readonly ILogger _logger;
    private readonly FlowiaDbContext _context;

    private IAccountRepository _accountRepository;
    private IMemberRepository _memberRepository;
    private IOwnerRepository _ownerRepository;
    private IRefreshTokenRepository _refreshTokenRepository;
    private IVerificationTokenRepository _verificationTokenRepository;
    private IResetPasswordTokenRepository _resetPasswordTokenRepository;
    
    public IAccountRepository Accounts => _accountRepository ??= new AccountRepository(_context);
    public IMemberRepository Members => _memberRepository ??= new MemberRepository(_context);
    public IOwnerRepository Owners => _ownerRepository ??= new OwnerRepository(_context);
    public IRefreshTokenRepository RefreshTokens => _refreshTokenRepository ??= new RefreshTokenRepository(_context);
    public IVerificationTokenRepository VerificationTokens => _verificationTokenRepository ??= new VerificationTokenRepository(_context);
    public IResetPasswordTokenRepository ResetPasswordTokens => _resetPasswordTokenRepository ??= new ResetPasswordTokenRepository(_context);
    
    protected UnitOfWork(
        FlowiaDbContext context)
    {
        _context = context;
        _logger = Log.ForContext("SourceContext", typeof(UnitOfWork).FullName);
    }
    
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
    {
        var result = await _context.SaveChangesAsync(cancellationToken);
            
        return result;
    }
    
    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }
    
    public ChangeTracker ChangeTracker => _context.ChangeTracker;
}