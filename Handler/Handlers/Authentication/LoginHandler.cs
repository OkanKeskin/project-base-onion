using Domain.Dtos.Authentication;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;
using Business.Services;
using System.Net;

namespace Handler.Handlers.Authentication;

public class LoginCommand : IRequest<AuthenticationResponse>
{
    public LoginRequest Request { get; set; }
}

public class LoginHandler : IRequestHandler<LoginCommand, AuthenticationResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;
    private readonly IConfiguration _configuration;
    private readonly IPasswordHashService _passwordHashService;

    public LoginHandler(
        IUnitOfWork unitOfWork,
        IAuthService authService,
        IConfiguration configuration,
        IPasswordHashService passwordHashService)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
        _configuration = configuration;
        _passwordHashService = passwordHashService;
    }

    public async Task<AuthenticationResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var account = await _unitOfWork.Accounts.GetByEmailAsync(request.Request.Email);
        
        if (account == null)
        {
            throw ApiException.Unauthorized("Geçersiz kullanıcı bilgileri");
        }

        // Verify password with hash
        if (!_passwordHashService.VerifyPassword(request.Request.Password, account.Password))
        {
            throw ApiException.Unauthorized("Geçersiz kullanıcı bilgileri");
        }

        var (accessToken, refreshToken) = await _authService.GenerateTokensAsync(account);

        return new AuthenticationResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiration = DateTime.UtcNow.AddMinutes(
                int.Parse(_configuration["JwtSettings:AccessTokenExpirationMinutes"])),
            AccountId = account.Id,
            Email = account.Email,
            AccountType = account.Type.ToString()
        };
    }
} 