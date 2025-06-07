using System.Security.Claims;
using Core.Contexts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Core.Base;

public class HttpUnitOfWork : UnitOfWork
{
    public HttpUnitOfWork(FlowiaDbContext context,
        IHttpContextAccessor httpAccessor) :
        base(context)
    {
        if (httpAccessor.HttpContext != null &&
            !string.IsNullOrEmpty(httpAccessor.HttpContext.User.FindFirst(ClaimTypes.Sid)?.Value))
            context.CurrentUserId = Guid.Parse(httpAccessor.HttpContext.User.FindFirst(ClaimTypes.Sid).Value);
    }
}