using System;
using Backend.Fx.Exceptions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.AspNetCore.Mvc.Throttling
{
    public class ExceptionThrottlingAttribute : ThrottlingBaseAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext actionContext)
        {
            var cache = actionContext.HttpContext.RequestServices.GetRequiredService<IMemoryCache>();
            var key = string.Concat(Name, "-", actionContext.HttpContext.Connection.RemoteIpAddress);

            if (actionContext.Exception == null)
            {
                cache.Remove(key);
                return;
            }

            if (cache.TryGetValue(key, out int repetition))
            {
                repetition++;
                var retryAfter = Math.Max(1, CalculateRepeatedTimeoutFactor(repetition)) * Seconds;
                cache.Set(key, repetition, TimeSpan.FromSeconds(retryAfter));
                throw new TooManyRequestsException(retryAfter,"Request canceled due to throttling", new Error("Throttled", string.Format(Message, retryAfter)));
            }

            cache.Set(key, 1, TimeSpan.FromSeconds(Seconds));
        }

        protected override int CalculateRepeatedTimeoutFactor(int repetition)
        {
            return repetition * repetition;
        }
    }
}