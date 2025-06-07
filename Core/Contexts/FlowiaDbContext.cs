using Core.EntityConfigs;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Core.Contexts;

public class FlowiaDbContext : BaseDbContext<Guid>
{

    public FlowiaDbContext(DbContextOptions options, IConfiguration configuration) : base(options,configuration)
    {
    }
    
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Member> Members { get; set; }
    public DbSet<Owner> Owners { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<VerificationToken> VerificationTokens { get; set; }
    public DbSet<ResetPasswordToken> ResetPasswordTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.ApplyConfiguration(new AccountEntityTypeConfiguration());
        builder.ApplyConfiguration(new OwnerEntityTypeConfiguration());
        builder.ApplyConfiguration(new MemberEntityTypeConfiguration());
        builder.ApplyConfiguration(new RefreshTokenEntityTypeConfiguration());
        builder.ApplyConfiguration(new VerificationTokenEntityTypeConfiguration());
        builder.ApplyConfiguration(new ResetPasswordTokenEntityTypeConfiguration());
    }
}