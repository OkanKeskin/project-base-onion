using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Domain.Dtos.Authentication;

public class RegisterRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    
    [Required]
    [MinLength(6)]
    public string Password { get; set; }
    
    [Required]
    [Compare("Password")]
    public string ConfirmPassword { get; set; }
    
    [Required]
    public string Name { get; set; }
    
    [Required]
    public string Surname { get; set; }
    
    public string? Gsm { get; set; }
    
    // Owner-specific fields
    public Gender? Gender { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Timezone { get; set; }
    public string? Photo { get; set; }
    
    [Required]
    public AccountType AccountType { get; set; }
} 