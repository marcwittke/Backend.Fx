using System;
using Backend.Fx.Exceptions;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.AspNetCore.Mvc.Throttling;

/// <summary>
/// returns HTTP 429 "Too many requests" when the attributed action gets called from the same IP address in less than
/// the configured interval. Useful to prevent denial of service attacks.
/// </summary>
[PublicAPI]
public class ThrottlingAttribute : ThrottlingBaseAttribute
{
    public override void OnActionExecuting(ActionExecutingContext actionContext)
    {
        var cache = actionContext.HttpContext.RequestServices.GetRequiredService<IMemoryCache>();
        var key = string.Concat(Name, "-", actionContext.HttpContext.Connection.RemoteIpAddress);

        if (cache.TryGetValue(key, out int repetition))
        {
            repetition++;
            var retryAfter = Math.Max(1, CalculateRepeatedTimeoutFactor(repetition)) * Seconds;
            cache.Set(key, repetition, TimeSpan.FromSeconds(retryAfter));
            throw new TooManyRequestsException(retryAfter).AddError(string.Format(Message, retryAfter));
        }

        cache.Set(key, 1, TimeSpan.FromSeconds(Seconds));
    }
}