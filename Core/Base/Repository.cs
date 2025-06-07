using System.Linq.Expressions;
using Core.Contexts;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Core.Base;

public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
{
    protected readonly FlowiaDbContext Context;
    private readonly DbSet<TEntity> _entities;
    
    private readonly ILogger _logger;
    
    protected Repository(FlowiaDbContext context)
    {
        Context = context;
        _entities = context.Set<TEntity>();
        _logger = Log.ForContext("SourceContext", typeof(Repository<TEntity>).FullName);
    }
    
    public virtual async Task AddAsync(TEntity entity)
    {
        await _entities.AddAsync(entity);
    }

    public virtual async Task AddRange(IEnumerable<TEntity> entities)
    {
        await _entities.AddRangeAsync(entities);
    }

    public virtual void Attach(TEntity entity)
    {
        _entities.Attach(entity);
    }

    public virtual void Update(TEntity entity)
    {
        _entities.Update(entity);
    }

    public virtual void UpdateRange(IEnumerable<TEntity> entities)
    {
        _entities.UpdateRange(entities);
    }

    public virtual void Remove(TEntity entity)
    {
        _entities.Remove(entity);
    }

    public virtual void RemoveRange(IEnumerable<TEntity> entities)
    {
        _entities.RemoveRange(entities);
    }

    public virtual IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate)
    {
        return _entities.Where(predicate);
    }

    public IAsyncEnumerable<TEntity> FindAsync(Expression<Func<TEntity, bool>> predicate,bool withAsNoTracking = false)
    {
        return withAsNoTracking ? _entities.Where(predicate).AsNoTracking().AsAsyncEnumerable() : _entities.Where(predicate).AsAsyncEnumerable();
    }

    public virtual Task<TEntity> GetSingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return _entities.SingleOrDefaultAsync(predicate);
    }

    public Task<TEntity> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return _entities.FirstOrDefaultAsync(predicate);
    }

    public virtual ValueTask<TEntity> GetAsync(Guid id)
    {
        return _entities.FindAsync(id);
    }

    public virtual IAsyncEnumerable<TEntity> GetAllAsync()
    {
        return _entities.AsAsyncEnumerable();
    }

    public async Task<bool> PartialUpdateAsync(TEntity entity, params Expression<Func<TEntity, object>>[] properties)
    {
        try
        {
            properties = properties.Distinct(new ExpressionComparer<TEntity>()).ToArray();
            
            var entry = Context.Entry(entity);
            Context.Set<TEntity>().Attach(entity);

            foreach (var property in properties)
                entry.Property(GetName(property)).IsModified = true;

            await Context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.Error("PartialUpdateAsync -> entity: {entity} exception: {Message} props: {props}",  entity.ToString(),ex.Message, properties );
            return false;
        }
    }

    private static string GetName<TSource, TField>(Expression<Func<TSource, TField>> field)
    {
        return (field.Body as MemberExpression
                ?? (MemberExpression) ((UnaryExpression) field.Body).Operand).Member.Name;
    }
    
    private class ExpressionComparer<T> : IEqualityComparer<Expression<Func<T, object>>>
    {
        public bool Equals(Expression<Func<T, object>> x, Expression<Func<T, object>> y)
        {
            return ExpressionComparerHelper.AreEqual(x, y);
        }

        public int GetHashCode(Expression<Func<T, object>> obj)
        {
            return ExpressionComparerHelper.GetHashCode(obj);
        }
    }

    private static class ExpressionComparerHelper
    {
        public static bool AreEqual<T>(Expression<Func<T, object>> x, Expression<Func<T, object>> y)
        {
            if (x == null || y == null)
                return false;

            return x.ToString() == y.ToString();  // This compares the expression bodies and parameters.
        }

        public static int GetHashCode<T>(Expression<Func<T, object>> expression)
        {
            return expression.ToString().GetHashCode(); // HashCode based on string representation of the expression.
        }
    }
}