using System.Threading.Tasks;
using Backend.Fx.Patterns.DependencyInjection;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace Backend.Fx.AspNetCore.Scoping
{
    public abstract class HoldCurrentMiddleware<T> : IMiddleware where T : class
    {
        private readonly ICurrentTHolder<T> _currentTHolder;

        [UsedImplicitly]
        protected HoldCurrentMiddleware(ICurrentTHolder<T> currentTHolder)
        {
            _currentTHolder = currentTHolder;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            T current = GetCurrent(context);
            _currentTHolder.ReplaceCurrent(current);
            await next.Invoke(context);
        }

        protected abstract T GetCurrent(HttpContext context);
    }
}