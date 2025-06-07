using System.Net;
using System.Text.Json.Serialization;

namespace Domain.Common;

public class ApiResponse<T>
{
    public T? Data { get; set; }
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    
    [JsonIgnore]
    public HttpStatusCode StatusCode { get; set; }
    
    public object? Errors { get; set; }

    // Başarılı response için factory metodu
    public static ApiResponse<T> Success(T data, string message = "İşlem başarılı", HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return new ApiResponse<T>
        {
            Data = data,
            IsSuccess = true,
            Message = message,
            StatusCode = statusCode,
            Errors = null
        };
    }

    // Başarısız response için factory metodu
    public static ApiResponse<T> Failure(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest, object? errors = null)
    {
        return new ApiResponse<T>
        {
            Data = default,
            IsSuccess = false,
            Message = message,
            StatusCode = statusCode,
            Errors = errors
        };
    }
}

// Veri dönmeyen API'ler için
public class ApiResponse : ApiResponse<object?>
{
    // Başarılı response için factory metodu
    public static ApiResponse Success(string message = "İşlem başarılı", HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return new ApiResponse
        {
            Data = null,
            IsSuccess = true,
            Message = message,
            StatusCode = statusCode,
            Errors = null
        };
    }

    // Başarısız response için factory metodu
    public static new ApiResponse Failure(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest, object? errors = null)
    {
        return new ApiResponse
        {
            Data = null,
            IsSuccess = false,
            Message = message,
            StatusCode = statusCode,
            Errors = errors
        };
    }
} 