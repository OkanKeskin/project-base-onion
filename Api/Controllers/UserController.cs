using System.Security.Claims;
using Business.Services;
using Domain.Common;
using Domain.Dtos.Authentication;
using Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Controllers;

[ApiController]
[Route("api/user-test")]
[Authorize]
public class UserController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly IAccountRepository _accountRepository;

    public UserController(
        IMediator mediator,
        IHttpContextAccessor contextAccessor) 
        : base(contextAccessor)
    {
        _mediator = mediator;
    }

    [HttpGet("profile")]
    [EnableRateLimiting("fixed")]
    public ActionResult<ApiResponse<UserProfileResponse>> GetProfile()
    {
        // Get user ID from claims (this is an example, in a real app you would query the database)
        var userId = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
        var email = User.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
        var accountType = User.Claims.FirstOrDefault(c => c.Type == "accountType")?.Value;
        
        var data = new UserProfileResponse
        {
            UserId = AccountId.ToString(),
            Email = email,
            AccountType = accountType,
            Role = Role
        };
        
        return Success(data, "Profil bilgileri başarıyla getirildi");
    }

    [HttpGet("admin-only")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("fixed")]
    public IActionResult AdminOnly()
    {
        return Ok(new { message = "This is an admin-only endpoint" });
    }
}

public class UserProfileResponse
{
    public string? UserId { get; set; }
    public string? Email { get; set; }
    public string? AccountType { get; set; }
    public string? Role { get; set; }
} 