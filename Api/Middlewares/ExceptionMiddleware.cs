using System.Net;
using System.Text.Json;
using Domain.Common;
using Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Api.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionMiddleware(RequestDelegate next, IHostEnvironment environment)
    {
        _next = next;
        _logger = Log.ForContext<ExceptionMiddleware>();
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        ApiResponse response;
        
        switch (exception)
        {
            case ApiException apiException:
                // Özel API hatası
                context.Response.StatusCode = (int)apiException.StatusCode;
                response = ApiResponse.Failure(
                    apiException.Message,
                    apiException.StatusCode,
                    apiException.Errors);
                break;
            
            case KeyNotFoundException:
                // Kaynak bulunamadı hatası
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response = ApiResponse.Failure(
                    exception.Message,
                    HttpStatusCode.NotFound);
                break;
                
            case UnauthorizedAccessException:
                // Yetkilendirme hatası
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response = ApiResponse.Failure(
                    exception.Message,
                    HttpStatusCode.Unauthorized);
                break;
            
            default:
                // Genel hata
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                
                // Geliştirme ortamında detaylı hata, production ortamında genel mesaj
                var message = _environment.IsDevelopment()
                    ? exception.Message
                    : "Bir hata oluştu. Lütfen daha sonra tekrar deneyin.";
                
                response = ApiResponse.Failure(
                    message,
                    HttpStatusCode.InternalServerError);
                
                // Beklenmeyen hatayı log'a yazdır
                _logger.Error(exception, "HATA: {@RequestPath}", context.Request.Path);
                break;
        }

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var json = JsonSerializer.Serialize(response, options);
        
        await context.Response.WriteAsync(json);
    }
} 