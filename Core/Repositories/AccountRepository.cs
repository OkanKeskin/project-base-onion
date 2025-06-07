using Core.Base;
using Core.Contexts;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Core.Repositories;

public class AccountRepository : Repository<Account>, IAccountRepository
{
    private readonly FlowiaDbContext _context;
    
    public AccountRepository(FlowiaDbContext context) : base(context)
    {
        _context = context;
    }
    
    public async Task<Account> GetByEmailAsync(string email)
    {
        return await _context.Set<Account>().FirstOrDefaultAsync(x => x.Email == email);
    }

    public async Task<Account> GetByIdAsync(Guid id)
    {
        return await _context.Set<Account>().FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<bool> IsEmailUniqueAsync(string email)
    {
        return !await _context.Accounts
            .AnyAsync(a => a.Email == email);
    }
}