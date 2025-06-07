using System.ComponentModel.DataAnnotations;

namespace Domain.Dtos.Authentication;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; }
} 