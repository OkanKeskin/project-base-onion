using Core.Base;
using Core.Contexts;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Core.Repositories;

public class OwnerRepository : Repository<Owner>, IOwnerRepository
{
    private readonly FlowiaDbContext _context;
    
    public OwnerRepository(FlowiaDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Owner> GetByAccountIdAsync(Guid accountId)
    {
        return await _context.Owners
            .FirstOrDefaultAsync(o => o.AccountId == accountId);
    }
} 