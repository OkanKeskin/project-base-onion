using Domain.Enums;

namespace Domain.Entities;

public class Owner
{
    public Guid AccountId { get; set; }
    
    public DateTime MembershipDate { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Email { get; set; }
    public string Gsm { get; set; }
    public Gender Gender { get; set; }
    public DateTime? BirthDate { get; set; }
    public string Timezone { get; set; }

    public string Photo { get; set; }
    
    public virtual Account Account { get; set; }
}