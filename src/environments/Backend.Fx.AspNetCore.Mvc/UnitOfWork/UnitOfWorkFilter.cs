using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.UnitOfWork;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Backend.Fx.AspNetCore.Mvc.UnitOfWork
{
    public class UnitOfWorkFilter : IActionFilter
    {
        private readonly IBackendFxApplication _application;

        public UnitOfWorkFilter(IBackendFxApplication application)
        {
            _application = application;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var unitOfWork = _application.CompositionRoot.GetInstance<IUnitOfWork>();
            if (context.HttpContext.Request.IsSafe())
            {
                unitOfWork = new ReadonlyDecorator(unitOfWork);
            }
            unitOfWork.Begin();
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            var unitOfWork = _application.CompositionRoot.GetInstance<IUnitOfWork>();

            try
            {
                unitOfWork.Complete();
            }
            finally
            {
                unitOfWork.Dispose();
            }
        }
    }
}
