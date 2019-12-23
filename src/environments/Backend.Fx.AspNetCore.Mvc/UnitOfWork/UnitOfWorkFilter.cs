using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.UnitOfWork;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Backend.Fx.AspNetCore.Mvc.UnitOfWork
{
    public class UnitOfWorkFilter : IActionFilter
    {
        private static readonly ILogger Logger = LogManager.Create<UnitOfWorkFilter>();
        private readonly IBackendFxApplication _application;

        public UnitOfWorkFilter(IBackendFxApplication application)
        {
            _application = application;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var unitOfWork = _application.CompositionRoot.GetInstance<IUnitOfWork>();
            unitOfWork.Begin();
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            var unitOfWork = _application.CompositionRoot.GetInstance<IUnitOfWork>();

            if (context.Exception == null)
            {
                unitOfWork.Complete();
            }
            else
            {
                Logger.Warn($"Preventing unit of work completion due to {context.Exception.GetType().Name}: {context.Exception.Message}");
            }
        }
    }
}
