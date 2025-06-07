using Domain.Common;
using Domain.Dtos.Authentication;
using Domain.Exceptions;
using Handler.Handlers.Authentication;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Controllers;

[Route("api/v1/auth")]
public class AuthController : BaseApiController
{
    private readonly IMediator _mediator;

    public AuthController(
        IMediator mediator,
        IHttpContextAccessor contextAccessor
        ): base(contextAccessor)
    {
        _mediator = mediator;
    }

    [HttpPost("login")]
    [EnableRateLimiting("fixed")]
    public async Task<ActionResult<ApiResponse<AuthenticationResponse>>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var command = new LoginCommand { Request = request };
            var response = await _mediator.Send(command);
            return Success(response, "Giriş başarılı");
        }
        catch (Exception ex)
        {
            return Failure<AuthenticationResponse>(ex.Message);
        }
    }

    /*
    [HttpPost("google-login")]
    [EnableRateLimiting("fixed")]
    public async Task<ActionResult<ApiResponse<AuthenticationResponse>>> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        try
        {
            var command = new GoogleLoginCommand { IdToken = request.IdToken };
            var response = await _mediator.Send(command);
            return Success(response, "Google ile giriş başarılı");
        }
        catch (Exception ex)
        {
            return Failure<AuthenticationResponse>(ex.Message);
        }
    }
    */

    [HttpPost("register")]
    [EnableRateLimiting("fixed")]
    public async Task<ActionResult<ApiResponse<AuthenticationResponse>>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var command = new RegisterCommand { Request = request };
            var response = await _mediator.Send(command);
            return Success(response, "Kayıt işlemi başarılı");
        }
        catch (Exception ex)
        {
            return Failure<AuthenticationResponse>(ex.Message);
        }
    }

    [HttpPost("refresh-token")]
    [EnableRateLimiting("sensitive")]
    public async Task<ActionResult<ApiResponse<AuthenticationResponse>>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var command = new RefreshTokenCommand { Request = request };
            var response = await _mediator.Send(command);
            return Success(response, "Token yenileme başarılı");
        }
        catch (Exception ex)
        {
            return Failure<AuthenticationResponse>(ex.Message);
        }
    }

    [HttpPost("forgot-password")]
    [EnableRateLimiting("fixed")]
    public async Task<ActionResult<ApiResponse<bool>>> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        try
        {
            var command = new ForgotPasswordCommand { Email = request.Email };
            var response = await _mediator.Send(command);
            return Success(response, "Şifre sıfırlama bağlantısı e-posta adresinize gönderildi");
        }
        catch (Exception ex)
        {
            return Failure<bool>(ex.Message);
        }
    }

    [HttpPost("reset-password")]
    [EnableRateLimiting("fixed")]
    public async Task<ActionResult<ApiResponse<bool>>> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        try
        {
            var command = new ResetPasswordCommand 
            { 
                Token = request.Token,
                Password = request.Password,
                ConfirmPassword = request.ConfirmPassword
            };
            var response = await _mediator.Send(command);
            return Success(response, "Şifreniz başarıyla sıfırlandı");
        }
        catch (Exception ex)
        {
            return Failure<bool>(ex.Message);
        }
    }

    [HttpPost("verify-email")]
    [EnableRateLimiting("fixed")]
    public async Task<ActionResult<ApiResponse<bool>>> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        try
        {
            var command = new VerifyEmailCommand 
            { 
                AccountId = request.AccountId,
                Token = request.Token
            };
            var response = await _mediator.Send(command);
            return Success(response, "E-posta adresiniz başarıyla doğrulandı");
        }
        catch (Exception ex)
        {
            return Failure<bool>(ex.Message);
        }
    }

    [HttpPost("send-verification-email")]
    [EnableRateLimiting("fixed")]
    public async Task<ActionResult<ApiResponse<bool>>> SendVerificationEmail([FromBody] SendVerificationEmailRequest request)
    {
        try
        {
            var command = new SendVerificationEmailCommand { Email = request.Email };
            var response = await _mediator.Send(command);
            return Success(response, "Doğrulama e-postası gönderildi");
        }
        catch (Exception ex)
        {
            return Failure<bool>(ex.Message);
        }
    }
} 