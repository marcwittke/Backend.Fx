using Backend.Fx.Exceptions;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Backend.Fx.AspNetCore.Mvc.Validation;

/// <summary>
/// Returns HTTP 400 "Bad Request" when model validation failed. In addition, the bad model state is converted
/// into an array of <see cref="Exceptions.SerializableError"/>s that is returned as JSON body, if the request
/// stated that it accepts JSON as response type content.
/// </summary>
[PublicAPI]
public class ReturnModelStateAsJsonModelValidationFilter : ModelValidationFilter
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ModelState.IsValid || !AcceptsJson(context)) return;
            
        Errors errors = context.ModelState.ToErrorsDictionary();
        LogErrors(context, context.Controller.ToString(), errors);
        context.Result = CreateResult(errors);
    }

    protected virtual IActionResult CreateResult(Errors errors)
    {
        return new JsonResult(errors.ToSerializableErrors());
    }

    public override void OnActionExecuted(ActionExecutedContext context)
    {
    }
}