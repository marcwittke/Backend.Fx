using System;

namespace Backend.Fx.Exceptions
{
    public class TooManyRequestsException : ClientException
    {
        public TooManyRequestsException(int retryAfter)
            : base("TooManyRequests")
        {
            RetryAfter = retryAfter;
        }

        public TooManyRequestsException(int retryAfter, string message) : base(message)
        {
            RetryAfter = retryAfter;
        }

        public TooManyRequestsException(int retryAfter, string message, Exception innerException) : base(message, innerException)
        {
            RetryAfter = retryAfter;
        }

        public int RetryAfter { get; }
    }
}
