using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.EntityConfigs;

public class MemberEntityTypeConfiguration: IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> entity)
    {
        entity.ToTable("Members");
        
        // Configure relationship with Account
        entity.HasOne(m => m.Account)
            .WithOne(a => a.Member)
            .HasForeignKey<Member>(m => m.AccountId);
        
        // Configure required fields
        entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
        entity.Property(e => e.Surname).IsRequired().HasMaxLength(100);
        entity.Property(e => e.Email).IsRequired().HasMaxLength(320);
    }
} 