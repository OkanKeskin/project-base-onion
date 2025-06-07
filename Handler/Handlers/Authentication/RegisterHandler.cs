using Domain.Dtos.Authentication;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;
using Business.Services;
using System.Net;

namespace Handler.Handlers.Authentication;

public class RegisterCommand : IRequest<AuthenticationResponse>
{
    public RegisterRequest Request { get; set; }
}

public class RegisterHandler : IRequestHandler<RegisterCommand, AuthenticationResponse>
{
    private readonly IAuthService _authService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly IPasswordHashService _passwordHashService;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;

    public RegisterHandler(
        IAuthService authService,
        IUnitOfWork unitOfWork,
        IConfiguration configuration,
        IPasswordHashService passwordHashService,
        ITokenService tokenService,
        IEmailService emailService)
    {
        _authService = authService;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _passwordHashService = passwordHashService;
        _tokenService = tokenService;
        _emailService = emailService;
    }

    public async Task<AuthenticationResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (!await _unitOfWork.Accounts.IsEmailUniqueAsync(request.Request.Email))
        {
            throw ApiException.Conflict("Bu e-posta adresi zaten kayıtlı");
        }

        // Hash password
        var hashedPassword = _passwordHashService.HashPassword(request.Request.Password);

        // Create account
        var account = new Account
        {
            Email = request.Request.Email,
            Password = hashedPassword, // Store hashed password
            Type = request.Request.AccountType,
            Provider = AccountProvider.Email,
            EmailVerification = VerificationStatus.Pending
        };

        await _unitOfWork.Accounts.AddAsync(account);
        
        // Create member or owner based on account type
        if (request.Request.AccountType == AccountType.Member)
        {
            var member = new Member
            {
                AccountId = account.Id,
                Name = request.Request.Name,
                Surname = request.Request.Surname,
                Email = request.Request.Email,
                Gsm = request.Request.Gsm ?? string.Empty,
                MembershipDate = DateTime.UtcNow
            };
            
            // Add member to database
            await _unitOfWork.Members.AddAsync(member);
        }
        else if (request.Request.AccountType == AccountType.Owner)
        {
            var owner = new Owner
            {
                AccountId = account.Id,
                Name = request.Request.Name,
                Surname = request.Request.Surname,
                Email = request.Request.Email,
                Gsm = request.Request.Gsm ?? string.Empty,
                MembershipDate = DateTime.UtcNow,
                Gender = request.Request.Gender ?? Gender.Unknown,
                BirthDate = request.Request.BirthDate,
                Timezone = request.Request.Timezone ?? "UTC",
                Photo = request.Request.Photo ?? string.Empty
            };
            
            // Add owner to database
            await _unitOfWork.Owners.AddAsync(owner);
        }
        
        // Create verification token
        var verificationToken = new VerificationToken
        {
            Token = _tokenService.GenerateRandomToken(),
            ExpiryDate = DateTime.UtcNow.AddHours(24),
            AccountId = account.Id,
            IsUsed = false
        };
        
        await _unitOfWork.VerificationTokens.AddAsync(verificationToken);
        await _unitOfWork.SaveChangesAsync();
        
        // Send verification email
        var verificationLink = $"{_configuration["Application:FrontendUrl"]}/verify-email?accountId={account.Id}&token={verificationToken.Token}";
        await _emailService.SendEmailVerificationAsync(account.Email, verificationLink);

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