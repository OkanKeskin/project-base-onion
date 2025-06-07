using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Domain.Common;
using Domain.Dtos.Authentication;
using Microsoft.Extensions.Localization;
using Localization.Resources;

namespace Api.Controllers;

[Route("api/v1/test")]
public class TokenTestController : BaseApiController
{
    private readonly IAuthService _authService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly IStringLocalizer<ErrorMessages> _localizer;

    public TokenTestController(
        IAuthService authService,
        IUnitOfWork unitOfWork,
        IConfiguration configuration,
        IHttpContextAccessor contextAccessor,
        IStringLocalizer<ErrorMessages> localizer)
        : base(contextAccessor)
    {
        _authService = authService;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _localizer = localizer;
    }

    [HttpPost("test-localization")]
    public IActionResult CreateTest(LoginRequest user)
    {
        var msg = _localizer["RequiredField"];
        return BadRequest(msg.Value);
    }

    [HttpGet("redirect-test")]
    public IActionResult RedirectTest()
    {
        // This endpoint redirects requests to Google
        // Example: https://localhost:5001/api/v1/test/redirect-test
        // Redirect URL: https://www.google.com
        return Ok(new { redirectUrl = "https://www.google.com" });
    }

    [HttpPost("validate-token")]
    [AllowAnonymous]
    [EnableRateLimiting("fixed")]
    public async Task<ActionResult<ApiResponse<TokenValidationResponse>>> ValidateToken([FromBody] string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return Failure<TokenValidationResponse>("Token is required");
        }

        var isValid = await _authService.ValidateTokenAsync(token);

        if (!isValid)
        {
            return Failure<TokenValidationResponse>("Token is invalid or expired");
        }

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            var claims = jwtToken.Claims.ToDictionary(c => c.Type, c => c.Value);

            var expiration = jwtToken.ValidTo;
            var currentTime = DateTime.UtcNow;
            var timeRemaining = expiration - currentTime;

            var data = new TokenValidationResponse
            {
                IsValid = true,
                TokenExpiration = expiration,
                CurrentTime = currentTime,
                TimeRemainingSeconds = timeRemaining.TotalSeconds,
                TimeRemainingMinutes = timeRemaining.TotalMinutes,
                Claims = claims
            };

            return Success(data, "Token doğrulandı");
        }
        catch (Exception ex)
        {
            return Failure<TokenValidationResponse>("Token incelenemedi: " + ex.Message);
        }
    }

    [HttpGet("token-info")]
    [Authorize]
    [EnableRateLimiting("fixed")]
    public async Task<IActionResult> GetTokenInfo()
    {
        var accessTokenExpirationMinutes = int.Parse(_configuration["JwtSettings:AccessTokenExpirationMinutes"]);
        var refreshTokenExpirationDays = int.Parse(_configuration["JwtSettings:RefreshTokenExpirationDays"]);

        var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub")?.Value;

        var authHeader = Request.Headers["Authorization"].ToString();
        var token = authHeader.Replace("Bearer ", "");

        var isValid = await _authService.ValidateTokenAsync(token);

        return Ok(new
        {
            AccountId = AccountId,
            UserId = userId,
            AccessTokenExpirationMinutes = accessTokenExpirationMinutes,
            RefreshTokenExpirationDays = refreshTokenExpirationDays,
            CurrentTime = DateTime.UtcNow,
            TokenIsValid = isValid,
            Message = "Bu endpoint token doğrulama süreci için test amaçlıdır."
        });
    }

    [HttpGet("token-details")]
    [Authorize]
    [EnableRateLimiting("fixed")]
    public IActionResult GetTokenDetails()
    {
        var authHeader = Request.Headers["Authorization"].ToString();
        var token = authHeader.Replace("Bearer ", "");

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            var claims = jwtToken.Claims.ToDictionary(c => c.Type, c => c.Value);

            var expiration = jwtToken.ValidTo;
            var currentTime = DateTime.UtcNow;
            var timeRemaining = expiration - currentTime;

            return Ok(new
            {
                AccountId = AccountId,
                TokenExpiration = expiration,
                CurrentTime = currentTime,
                TimeRemainingSeconds = timeRemaining.TotalSeconds,
                TimeRemainingMinutes = timeRemaining.TotalMinutes,
                IsExpired = DateTime.UtcNow > expiration,
                Claims = claims,
                FullToken = token
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = "Token incelenemedi", Message = ex.Message });
        }
    }

    [HttpGet("wait-for-expiration")]
    [Authorize]
    [EnableRateLimiting("fixed")]
    public IActionResult WaitForExpiration()
    {
        var accessTokenExpirationMinutes = int.Parse(_configuration["JwtSettings:AccessTokenExpirationMinutes"]);
        return Ok(new
        {
            Message = $"Access token {accessTokenExpirationMinutes} dakika içinde expire olacaktır. Lütfen bekleyiniz.",
            ExpirationTimeInMinutes = accessTokenExpirationMinutes,
            CurrentTime = DateTime.UtcNow
        });
    }

    [HttpGet("test-expired")]
    [Authorize(Roles = "Member")]
    [EnableRateLimiting("fixed")]
    public IActionResult TestExpired()
    {
        return Ok(new
        {
            Message = "Token hala geçerli!",
            CurrentTime = DateTime.UtcNow
        });
    }

    [HttpGet("check-token")]
    [AllowAnonymous]
    [EnableRateLimiting("fixed")]
    public async Task<ActionResult<ApiResponse<TokenStatusResponse>>> CheckToken([FromQuery] string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                token = authHeader.Replace("Bearer ", "");
            }
            else
            {
                return Failure<TokenStatusResponse>("Token is required");
            }
        }

        var isValid = await _authService.ValidateTokenAsync(token);

        var data = new TokenStatusResponse
        {
            IsValid = isValid,
            CurrentTime = DateTime.UtcNow
        };

        return Success(data, isValid ? "Token geçerli" : "Token geçersiz veya süresi dolmuş");
    }
}

public class TokenValidationResponse
{
    public bool IsValid { get; set; }
    public DateTime TokenExpiration { get; set; }
    public DateTime CurrentTime { get; set; }
    public double TimeRemainingSeconds { get; set; }
    public double TimeRemainingMinutes { get; set; }
    public Dictionary<string, string>? Claims { get; set; }
}

public class TokenStatusResponse
{
    public bool IsValid { get; set; }
    public DateTime CurrentTime { get; set; }
}