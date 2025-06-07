using Core.Base;
using Core.Contexts;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Core.Repositories;

public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
{
    private readonly FlowiaDbContext _context;
    
    public RefreshTokenRepository(FlowiaDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<RefreshToken> GetByTokenAsync(string token)
    {
        return await _context.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == token);
    }

    public async Task<IEnumerable<RefreshToken>> GetByAccountIdAsync(Guid accountId)
    {
        return await _context.RefreshTokens
            .Where(r => r.AccountId == accountId)
            .ToListAsync();
    }
} 