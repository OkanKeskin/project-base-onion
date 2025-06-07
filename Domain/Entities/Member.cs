using Domain.Base;

namespace Domain.Entities;

public class Member : AuditableEntity
{
    public Guid AccountId { get; set; }
    
    public DateTime MembershipDate { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Email { get; set; }
    public string Gsm { get; set; }
    
    public virtual Account Account { get; set; }
}