using Domain.Entities;

namespace Domain.Interfaces;

public interface IAccountRepository : IRepository<Account>
{
    Task<Account> GetByEmailAsync(string email);
    Task<Account> GetByIdAsync(Guid id);
    Task<bool> IsEmailUniqueAsync(string email);
}