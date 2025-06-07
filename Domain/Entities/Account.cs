using Domain.Base;
using Domain.Enums;

namespace Domain.Entities;

public class Account : AuditableEntity
{
    public AccountProvider Provider { get; set; }
    public AccountType Type { get; set; }
    public VerificationStatus EmailVerification { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    
    public virtual Member Member { get; set; }
    public virtual Owner Owner { get; set; }
}