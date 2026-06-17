using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SchoolERPManagementDALLibrary.Contexts;
using System.Security.Claims;

namespace SchoolERPManagementAPI.Filters;

public class ActiveUserFilterAttribute : IAsyncActionFilter
{
    private readonly SchoolERPDbContext _context;

    public ActiveUserFilterAttribute(SchoolERPDbContext context)
    {
        _context = context;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var userPrincipal = context.HttpContext.User;
        if (userPrincipal.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = userPrincipal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                var user = await _context.Users.FindAsync(userId);
                
                // If user doesn't exist, or their active status is explicitly false, reject them.
                if (user == null || user.Isactive == false)
                {
                    context.Result = new ObjectResult(new { Message = "Your account is deactivated." })
                    {
                        StatusCode = StatusCodes.Status403Forbidden
                    };
                    return;
                }
            }
        }

        // Proceed to the controller action
        await next();
    }
}
