using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.EntityConfigs;

public class ResetPasswordTokenEntityTypeConfiguration : IEntityTypeConfiguration<ResetPasswordToken>
{
    public void Configure(EntityTypeBuilder<ResetPasswordToken> entity)
    {
        entity.ToTable("ResetPasswordTokens");
        
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