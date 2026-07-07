using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using SchoolERPManagementDALLibrary.Contexts;

namespace SchoolERPManagementAPI.Middlewares;

public class UserLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public UserLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Extract username from claims
        string? username = null;
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            // Try unique_name (standard for Jwt unique name) first, then Name, then NameIdentifier
            username = context.User.FindFirst("unique_name")?.Value 
                       ?? context.User.FindFirst(ClaimTypes.Name)?.Value 
                       ?? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        // Default to Anonymous if not authenticated
        CurrentUserContext.Username = username ?? "Anonymous";

        try
        {
            await _next(context);
        }
        finally
        {
            // Clear context after request finishes to avoid leaking across requests
            CurrentUserContext.Username = null;
        }
    }
}
