using Backend.Fx.Environment.Persistence;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.AspNetCore.Mvc.Execution
{
    /// <summary>
    /// Makes sure that possible dirty objects are flushed to the persistence layer when the MVC action was executed. This will reveal
    /// persistence related problems early and makes them easier to diagnose.
    /// </summary>
    public class FlushFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        { }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            context.HttpContext.GetServiceProvider().GetRequiredService<ICanFlush>().Flush();
        }
    }
}