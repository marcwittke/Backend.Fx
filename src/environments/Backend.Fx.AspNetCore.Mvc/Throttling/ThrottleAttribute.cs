using System;
using Backend.Fx.Exceptions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.AspNetCore.Mvc.Throttling
{
    public class ThrottleAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// A unique name for this Throttle.
        /// </summary>
        /// <remarks>
        /// We'll be inserting a Cache record based on this name and client IP, e.g. "Name-192.168.0.1"
        /// </remarks>
        public string Name { get; set; }

        /// <summary>
        /// The number of seconds clients must wait before executing this decorated route again.
        /// </summary>
        public int Seconds { get; set; }

        /// <summary>
        /// A text message that will be sent to the client upon throttling.  You can include the token {n} to
        /// show this.Seconds in the message, e.g. "Wait {n} seconds before trying again".
        /// </summary>
        public string Message { get; set; }

        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            var cache = actionContext.HttpContext.RequestServices.GetRequiredService<IMemoryCache>();
            var key = string.Concat(Name, "-", actionContext.HttpContext.Connection.RemoteIpAddress);

            if (cache.TryGetValue(key, out bool allowExecute) && !allowExecute)
            {
                if (string.IsNullOrEmpty(Message))
                {
                    Message = "You may only perform this action every {0} seconds.";
                }

                throw new TooManyRequestsException("Request canceled due to throttling", new Error("TooManyRequests", string.Format(Message, Seconds)));
            }

            cache.Set(key, false, TimeSpan.FromSeconds(Seconds));
        }
    }
}
