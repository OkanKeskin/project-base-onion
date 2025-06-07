using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.EntityConfigs;

public class OwnerEntityTypeConfiguration: IEntityTypeConfiguration<Owner>
{
    public void Configure(EntityTypeBuilder<Owner> entity)
    {
        entity.ToTable("Owners");
        
        // Set AccountId as primary key
        entity.HasKey(o => o.AccountId);
        
        // Configure relationship with Account
        entity.HasOne(o => o.Account)
            .WithOne(a => a.Owner)
            .HasForeignKey<Owner>(o => o.AccountId);
        
        // Configure required fields
        entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
        entity.Property(e => e.Surname).IsRequired().HasMaxLength(100);
        entity.Property(e => e.Email).IsRequired().HasMaxLength(320);
    }
} 