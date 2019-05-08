using System.Diagnostics;
using Backend.Fx.AspNetCore.UnitOfWork;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.UnitOfWork;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Backend.Fx.AspNetCore.Mvc.ErrorHandling
{
    /// <summary>
    /// Enforces flushing of db changes when an action was executed, so that possible schema violation errors
    /// are raised before the http response starts, and our exception handling can still change the response.
    /// </summary>
    /// <remarks>This cannot be done in a middleware. MVC will start the response as soon as the controller
    /// action returns. The <see cref="UnitOfWorkMiddleware"/> would commit changes
    /// too late for the <see cref="Backend.Fx.AspNetCore.ErrorHandling.JsonErrorHandlingMiddleware"/> to
    /// manipulate the response status.</remarks>
    [DebuggerStepThrough]
    public class FlushFilter : IActionFilter
    {
        private readonly IBackendFxApplication _backendFxApplication;

        public FlushFilter(IBackendFxApplication backendFxApplication)
        {
            _backendFxApplication = backendFxApplication;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        { }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // only unsafe requests need flush
            if (context.HttpContext.Request.IsSafe())
            {
                return;
            }

            // that's all:
            _backendFxApplication.CompositionRoot.GetInstance<ICanFlush>().Flush();
        }
    }
}
