using System.Linq.Expressions;

namespace Domain.Interfaces;

public interface IRepository<TEntity> where TEntity : class
{
    Task AddAsync(TEntity entity);
    Task AddRange(IEnumerable<TEntity> entities);

    void Attach(TEntity entity);
    void Update(TEntity entity);
    void UpdateRange(IEnumerable<TEntity> entities);

    void Remove(TEntity entity);
    void RemoveRange(IEnumerable<TEntity> entities);

    IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate);
    IAsyncEnumerable<TEntity> FindAsync(Expression<Func<TEntity, bool>> predicate, bool withAsNoTracking = false);
    Task<TEntity> GetSingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);
    Task<TEntity> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);
    ValueTask<TEntity> GetAsync(Guid id);
    Task<bool> PartialUpdateAsync(TEntity entity, params Expression<Func<TEntity, object>>[] properties);
    IAsyncEnumerable<TEntity> GetAllAsync();
}