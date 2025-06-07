using System.Net;

namespace Domain.Exceptions;

public class ApiException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public object? Errors { get; } 

    public ApiException(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest, object? errors = null) 
        : base(message)
    {
        StatusCode = statusCode;
        Errors = errors;
    }

    public ApiException(string message, Exception innerException, HttpStatusCode statusCode = HttpStatusCode.BadRequest, object? errors = null) 
        : base(message, innerException)
    {
        StatusCode = statusCode;
        Errors = errors;
    }
    
    // Özel durum türleri
    public static ApiException NotFound(string message = "Resource not found", object? errors = null)
    {
        return new ApiException(message, HttpStatusCode.NotFound, errors);
    }
    
    public static ApiException Unauthorized(string message = "Unauthorized access", object? errors = null)
    {
        return new ApiException(message, HttpStatusCode.Unauthorized, errors);
    }
    
    public static ApiException Forbidden(string message = "Access forbidden", object? errors = null)
    {
        return new ApiException(message, HttpStatusCode.Forbidden, errors);
    }
    
    public static ApiException BadRequest(string message = "Invalid request", object? errors = null)
    {
        return new ApiException(message, HttpStatusCode.BadRequest, errors);
    }
    
    public static ApiException InternalServerError(string message = "An unexpected error occurred", object? errors = null)
    {
        return new ApiException(message, HttpStatusCode.InternalServerError, errors);
    }
    
    public static ApiException Conflict(string message = "Conflict with current state", object? errors = null)
    {
        return new ApiException(message, HttpStatusCode.Conflict, errors);
    }
} 