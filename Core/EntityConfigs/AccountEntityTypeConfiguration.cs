using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.EntityConfigs;

public class AccountEntityTypeConfiguration: IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> entity)
    {
        entity.ToTable("Accounts");
        
        entity
            .HasIndex(u => u.Id)
            .IsUnique();
        
        entity.Property(e => e.Email).IsRequired().HasMaxLength(320);
    }
}