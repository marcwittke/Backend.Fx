namespace Backend.Fx.AspNetCore.Scoping
{
    using System.Diagnostics;
    using Bootstrapping;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Patterns.UnitOfWork;

    /// <summary>
    /// Enforces flushing of db changes when an action was executed, so that possible schema violation errors
    /// are raised before the http repsone starts, and our exception handling can still change the response.
    /// </summary>
    [DebuggerStepThrough]
    public class FlushFilter : IActionFilter
    {
        private readonly IBackendFxApplication mepApplication;

        public FlushFilter(IBackendFxApplication mepApplication)
        {
            this.mepApplication = mepApplication;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        { }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // that's all:
            mepApplication.CompositionRoot.GetInstance<ICanFlush>().Flush();
        }
    }
}