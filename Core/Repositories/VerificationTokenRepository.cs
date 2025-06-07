using Core.Base;
using Core.Contexts;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Core.Repositories;

public class VerificationTokenRepository : Repository<VerificationToken>, IVerificationTokenRepository
{
    private readonly FlowiaDbContext _context;
    
    public VerificationTokenRepository(FlowiaDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<VerificationToken> GetByTokenAsync(string token)
    {
        return await _context.VerificationTokens
            .FirstOrDefaultAsync(r => r.Token == token);
    }

    public async Task<VerificationToken> GetLatestByAccountIdAsync(Guid accountId)
    {
        return await _context.VerificationTokens
            .Where(r => r.AccountId == accountId)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync();
    }
} 