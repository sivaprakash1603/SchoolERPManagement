using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace SchoolERPManagementAPI.Filters;

public class ValidationFilterAttribute : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var methodParameters = context.ActionDescriptor.Parameters;

        foreach (var parameter in methodParameters)
        {
            if (context.ActionArguments.TryGetValue(parameter.Name, out var argument) && argument != null)
            {
                var argumentType = argument.GetType();
                
                // Construct the generic validator type: IValidator<T>
                var validatorType = typeof(IValidator<>).MakeGenericType(argumentType);

                // Attempt to resolve the validator from DI
                var validator = context.HttpContext.RequestServices.GetService(validatorType) as IValidator;

                if (validator != null)
                {
                    var validationContext = new ValidationContext<object>(argument);
                    var validationResult = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);

                    if (!validationResult.IsValid)
                    {
                        var errors = validationResult.Errors
                            .GroupBy(e => e.PropertyName)
                            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                        context.Result = new BadRequestObjectResult(new { Errors = errors, Message = "One or more validation errors occurred." });
                        return;
                    }
                }
            }
        }

        await next();
    }
}
