using System.Threading.Tasks;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.UnitOfWork;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Backend.Fx.AspNetCore.Mvc.UnitOfWork
{
    public class UnitOfWorkFilter : IAsyncActionFilter
    {
        private readonly IBackendFxApplication _application;

        public UnitOfWorkFilter(IBackendFxApplication application)
        {
            _application = application;
        }
        
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var unitOfWork = _application.CompositionRoot.GetInstance<IUnitOfWork>();
            try
            {
                if (context.HttpContext.Request.IsSafe())
                {
                    unitOfWork = new ReadonlyDecorator(unitOfWork);
                }

                unitOfWork.Begin();
                await next();
                await unitOfWork.CompleteAsync();

            }
            finally
            {
                unitOfWork.Dispose();
            }
        }
    }
}