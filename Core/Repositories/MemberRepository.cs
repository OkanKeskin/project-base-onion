using Core.Base;
using Core.Contexts;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Core.Repositories;

public class MemberRepository : Repository<Member>, IMemberRepository
{
    private readonly FlowiaDbContext _context;
    
    public MemberRepository(FlowiaDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Member> GetByAccountIdAsync(Guid accountId)
    {
        return await _context.Members
            .FirstOrDefaultAsync(m => m.AccountId == accountId);
    }
} 