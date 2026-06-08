using System.Net;
using System.Text.Json;
using SchoolERPManagementBLLibrary.Exceptions;

namespace SchoolERPManagementAPI.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception has occurred while executing the request.");
            await HandleExceptionAsync(context, ex, _env);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception, IHostEnvironment env)
    {
        context.Response.ContentType = "application/json";

        context.Response.StatusCode = exception switch
        {
            EntityNotFoundException => (int)HttpStatusCode.NotFound,
            DuplicateEntityException => (int)HttpStatusCode.Conflict,
            BusinessRuleException => (int)HttpStatusCode.BadRequest,
            UnauthorizedAccessException => (int)HttpStatusCode.Forbidden,
            _ => (int)HttpStatusCode.InternalServerError
        };

        var response = new
        {
            StatusCode = context.Response.StatusCode,
            Message = exception.Message,
            Detailed = env.IsDevelopment() ? exception.StackTrace : null
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
