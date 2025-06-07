using Domain.Base;

namespace Domain.Entities;

public class RefreshToken : AuditableEntity
{
    public string Token { get; set; }
    public DateTime ExpiryDate { get; set; }
    public bool IsRevoked { get; set; }
    public bool IsUsed { get; set; }
    public Guid AccountId { get; set; }
    
    public virtual Account Account { get; set; }
} 