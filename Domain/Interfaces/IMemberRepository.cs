using Domain.Entities;

namespace Domain.Interfaces;

public interface IMemberRepository : IRepository<Member>
{
    Task<Member> GetByAccountIdAsync(Guid accountId);
} 