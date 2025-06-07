using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Core.Contexts;

public class BaseDbContext<TKey> : DbContext where TKey : IEquatable<TKey>
{
    public Guid CurrentUserId { get; set; }
    private IConfiguration Configuration { get; }
    
    public BaseDbContext(DbContextOptions options, IConfiguration configuration) : base(options)
    {
        Configuration = configuration;
    }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.EnableSoftDelete();

        base.OnModelCreating(builder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.LogTo(Console.WriteLine);
    }
}

public static class ModelBuilderExtensions
{
    public static ModelBuilder EnableSoftDelete(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // 1. Add the IsDeleted property
            var hasIsDeletedProperty = entityType.FindProperty("is_deleted");

            if (hasIsDeletedProperty == null)
            {
                var prop = entityType.AddProperty("is_deleted", typeof(bool));
                prop.SetDefaultValue(false);
            }

            // 2. Create the query filter
            var parameter = Expression.Parameter(entityType.ClrType);

            // EF.Property<bool>(post, "is_deleted")
            var propertyMethodInfo = typeof(EF).GetMethod("Property")?.MakeGenericMethod(typeof(bool));

            if (propertyMethodInfo == null)
                throw new ArgumentException("Error on constructing soft delete expression");

            var isDeletedProperty = Expression.Call(
                propertyMethodInfo,
                parameter,
                Expression.Constant("is_deleted")
            );

            // EF.Property<bool>(post, "is_deleted") == false
            var compareExpression = Expression.MakeBinary(
                ExpressionType.Equal,
                isDeletedProperty,
                Expression.Constant(false)
            );

            // post => EF.Property<bool>(post, "is_deleted") == false
            var lambda = Expression.Lambda(compareExpression, parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }

        return modelBuilder;
    }
}