using Backend.Fx.Environment.Persistence;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Backend.Fx.AspNetCore.Mvc.Execution
{
    public class FlushFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        { }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            context.HttpContext.GetInstanceProvider().GetInstance<ICanFlush>().Flush();
        }
    }
}