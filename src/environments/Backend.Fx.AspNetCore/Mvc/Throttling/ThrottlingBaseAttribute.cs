using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Backend.Fx.AspNetCore.Mvc.Throttling
{
    public abstract class ThrottlingBaseAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// A unique name for this Throttle.
        /// </summary>
        /// <remarks>
        /// We'll be inserting a Cache record based on this name and client IP, e.g. "Name-192.168.0.1"
        /// </remarks>
        [UsedImplicitly]
        public string Name { get; set; }

        /// <summary>
        /// The number of seconds clients must wait before executing this decorated route again.
        /// </summary>
        [UsedImplicitly]
        public int Seconds { get; set; }

        /// <summary>
        /// A text message that will be sent to the client upon throttling. You can include the token {0} to
        /// show this.Seconds in the message, e.g. "Wait {0} seconds before trying again".
        /// </summary>
        [UsedImplicitly]
        public string Message { get; set; } = "Wait {0} seconds before trying again";

        protected virtual int CalculateRepeatedTimeoutFactor(int repetition)
        {
            return 1;
        }
    }
}
