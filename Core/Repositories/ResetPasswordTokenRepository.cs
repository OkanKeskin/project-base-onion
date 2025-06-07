using Core.Base;
using Core.Contexts;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Core.Repositories;

public class ResetPasswordTokenRepository : Repository<ResetPasswordToken>, IResetPasswordTokenRepository
{
    private readonly FlowiaDbContext _context;
    
    public ResetPasswordTokenRepository(FlowiaDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<ResetPasswordToken> GetByTokenAsync(string token)
    {
        return await _context.ResetPasswordTokens
            .FirstOrDefaultAsync(r => r.Token == token);
    }

    public async Task<ResetPasswordToken> GetLatestByAccountIdAsync(Guid accountId)
    {
        return await _context.ResetPasswordTokens
            .Where(r => r.AccountId == accountId)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync();
    }
} 