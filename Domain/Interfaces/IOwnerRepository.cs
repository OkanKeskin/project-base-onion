using Domain.Entities;

namespace Domain.Interfaces;

public interface IOwnerRepository : IRepository<Owner>
{
    Task<Owner> GetByAccountIdAsync(Guid accountId);
} 