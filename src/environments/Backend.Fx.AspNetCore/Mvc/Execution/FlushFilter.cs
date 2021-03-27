using Backend.Fx.Environment.Persistence;
using Backend.Fx.Patterns.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Backend.Fx.AspNetCore.Mvc.Execution
{
    public class FlushFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        { }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            ((IInstanceProvider) context.HttpContext.Items[HttpContextItemKey.InstanceProvider]).GetInstance<ICanFlush>().Flush();
        }
    }
}