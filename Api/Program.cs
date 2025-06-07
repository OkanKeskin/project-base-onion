using Bootstrapper;
using Core.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Microsoft.Extensions.Options;
using Api.Middlewares;
using FluentValidation.AspNetCore;

namespace Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers().AddFluentValidation(config =>
            {
                config.AutomaticValidationEnabled = false; // Varsayılan doğrulama devre dışı
            });;
        
        // Add rate limiting
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            
            // Add a fixed window rate limiter
            options.AddFixedWindowLimiter("fixed", opt =>
            {
                opt.Window = TimeSpan.FromMinutes(1);
                opt.PermitLimit = 10;
                opt.QueueLimit = 0;
            });
            
            // Add a sliding window rate limiter for sensitive operations
            options.AddSlidingWindowLimiter("sensitive", opt =>
            {
                opt.Window = TimeSpan.FromMinutes(1);
                opt.PermitLimit = 5;
                opt.SegmentsPerWindow = 4;
                opt.QueueLimit = 0;
            });
        });
        
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Flowia API", Version = "v1" });
            
            // Define the security scheme
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });
            
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
        
        // Add HttpContextAccessor
        builder.Services.AddHttpContextAccessor();
        
        // Add security headers
        builder.Services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");
        
        StartupConfigurationExtensions.AddDbContext(builder.Services, builder.Configuration);
        StartupConfigurationExtensions.AddServices(builder.Services);
        StartupConfigurationExtensions.AddCqrs(builder.Services);
        StartupConfigurationExtensions.AddJwtAuthentication(builder.Services, builder.Configuration);
        StartupConfigurationExtensions.AddCors(builder.Services, builder.Configuration);
        StartupConfigurationExtensions.AddLocalization(builder.Services, builder.Configuration);
        StartupConfigurationExtensions.AddValidation(builder.Services, builder.Configuration);

        var app = builder.Build();

        // Configure global exception handling
        app.UseMiddleware<ExceptionMiddleware>();

       // Kültür bilgilerini CultureInfo ile belirtmelisin
        var supportedCultures = new[] { "en", "tr" }
        .Select(c => new CultureInfo(c))
        .ToArray();

        // Lokalizasyon middleware'ini ayarlıyoruz
        app.UseRequestLocalization(new RequestLocalizationOptions
        {
            DefaultRequestCulture = new RequestCulture("en"),
            SupportedCultures = supportedCultures,
            SupportedUICultures = supportedCultures
        });

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        else 
        {
            // Add security headers in production
            app.UseHsts();
        }

        // Add security headers
        app.UseHttpsRedirection();
        
        // Add security headers middleware
        app.Use(async (context, next) =>
        {
            // Prevent XSS attacks
            context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
            
            // Prevent MIME type sniffing
            context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
            
            // Control iframe usage to prevent clickjacking
            context.Response.Headers.Add("X-Frame-Options", "DENY");
            
            // Implement Content Security Policy
            context.Response.Headers.Add(
                "Content-Security-Policy", 
                "default-src 'self'; script-src 'self'; object-src 'none'; img-src 'self' data:; style-src 'self'; font-src 'self'; connect-src 'self'");
            
            // Referrer Policy - limit information sent in the Referer header
            context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
            
            // Permissions Policy - control browser features
            context.Response.Headers.Add(
                "Permissions-Policy", 
                "camera=(), microphone=(), geolocation=(), interest-cohort=()");
            
            await next();
        });

        app.UseCors("AllowFrontend");
        
        // Add authentication and authorization middlewares
        app.UseAuthentication();
        app.UseAuthorization();

        // Use rate limiting middleware
        app.UseRateLimiter();

        app.MapControllers();

        app.Run();
    }
}