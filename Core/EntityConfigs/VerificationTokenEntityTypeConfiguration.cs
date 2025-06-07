using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.EntityConfigs;

public class VerificationTokenEntityTypeConfiguration : IEntityTypeConfiguration<VerificationToken>
{
    public void Configure(EntityTypeBuilder<VerificationToken> entity)
    {
        entity.ToTable("VerificationTokens");
        
        entity
            .HasIndex(u => u.Id)
            .IsUnique();
        
        entity.Property(e => e.Token).IsRequired();
        entity.Property(e => e.ExpiryDate).IsRequired();
        
        // Configure relationship with Account
        entity.HasOne(rt => rt.Account)
            .WithMany()
            .HasForeignKey(rt => rt.AccountId);
    }
} 