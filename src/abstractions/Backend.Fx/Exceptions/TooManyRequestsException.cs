using System;

namespace Backend.Fx.Exceptions
{
    public class TooManyRequestsException : ClientException
    {
        public TooManyRequestsException(int retryAfter)
        {
            RetryAfter = retryAfter;
        }

        public TooManyRequestsException(int retryAfter, params Error[] errors) : base(errors)
        {
            RetryAfter = retryAfter;
        }

        public TooManyRequestsException(int retryAfter, string message, params Error[] errors) : base(message, errors)
        {
            RetryAfter = retryAfter;
        }

        public TooManyRequestsException(int retryAfter, string message, Exception innerException, params Error[] errors) : base(message, innerException, errors)
        {
            RetryAfter = retryAfter;
        }

        public int RetryAfter { get; }
    }
}
