using System.Reflection;
using System.Text;
using Business.Services;
using Core.Base;
using Core.Contexts;
using Core.Repositories;
using Domain.Interfaces;
using Domain.Validations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MediatR;
using FluentValidation;
using Microsoft.AspNetCore.Http;
namespace Bootstrapper;

public static class StartupConfigurationExtensions
{
    public static IServiceCollection AddDbContext(this IServiceCollection services,IConfiguration configuration)
    {
        services.AddDbContext<FlowiaDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        return services;
    }
    
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        #region unit of work

        services.AddScoped<IUnitOfWork, HttpUnitOfWork>();

        #endregion
        
        #region database

        services.AddScoped<DbContext, FlowiaDbContext>();

        #endregion
        
        #region services
        
        services.AddHttpContextAccessor();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPasswordHashService, PasswordHashService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IGoogleAuthService, GoogleAuthService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IS3Service, S3Service>();
        
        // Add HTTP client factory for GoogleAuthService
        services.AddHttpClient();
        
        #endregion

        return services;
    }

    public static IServiceCollection AddCqrs(this IServiceCollection services)
    {
        // Register mediator handlers from the Handler assembly
        var assemblies = new[]
        {
            Assembly.GetExecutingAssembly(), // Bootstrapper assembly
            Assembly.Load("Handler") // Handler assembly
        };
        
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(assemblies));
        
        return services;
    }
    
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidIssuer = configuration["JwtSettings:Issuer"],
                    ValidAudience = configuration["JwtSettings:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["JwtSettings:SecretKey"]))
                };
            });
        
        return services;
    }

    public static IServiceCollection AddCors(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(options => options.AddPolicy("AllowFrontend",
            policy =>
            {
                policy.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            }));

        return services;
    }

    public static IServiceCollection AddLocalization(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddLocalization(options=>options.ResourcesPath = "");

        return services;
    }

    public static IServiceCollection AddValidation(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddValidatorsFromAssemblyContaining<UserValidator>();

        return services;
    }
}