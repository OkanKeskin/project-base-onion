using System.Net;
using System.Security.Claims;
using Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace Api.Controllers;

[ApiController]
[Produces("application/json")]
public class BaseApiController : ControllerBase
{
    [NonAction]
    protected ActionResult<ApiResponse<T>> HandleResponse<T>(ApiResponse<T> response)
    {
        return StatusCode((int)response.StatusCode, response);
    }

    [NonAction]
    protected ActionResult<ApiResponse<T>> Success<T>(T data, string message = "İşlem başarılı", HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return HandleResponse(ApiResponse<T>.Success(data, message, statusCode));
    }

    [NonAction]
    protected ActionResult<ApiResponse<T>> Failure<T>(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest, object? errors = null)
    {
        return HandleResponse(ApiResponse<T>.Failure(message, statusCode, errors));
    }
    
    [NonAction]
    protected ActionResult<ApiResponse> Success(string message = "İşlem başarılı", HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var response = ApiResponse.Success(message, statusCode);
        return StatusCode((int)response.StatusCode, response);
    }

    [NonAction]
    protected ActionResult<ApiResponse> Failure(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest, object? errors = null)
    {
        var response = ApiResponse.Failure(message, statusCode, errors);
        return StatusCode((int)response.StatusCode, response);
    }

    private readonly IMediator _mediator;
    private readonly IHttpContextAccessor _contextAccessor;

    protected BaseApiController(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    protected Guid AccountId
    {
        get
        {
            try
            {
                if (_contextAccessor?.HttpContext?.User?.Identity?.IsAuthenticated == true)
                {
                    var sidClaim = _contextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Sid);
                    if (!string.IsNullOrEmpty(sidClaim))
                    {
                        return Guid.Parse(sidClaim);
                    }
                }
                return Guid.Empty;
            }
            catch
            {
                return Guid.Empty;
            }
        }
    }
    
    protected string Role => _contextAccessor.HttpContext.User != null
        ? _contextAccessor.HttpContext.User.FindFirst(x=>x.Type == ClaimTypes.Role)?.Value
        : string.Empty;
    
} 