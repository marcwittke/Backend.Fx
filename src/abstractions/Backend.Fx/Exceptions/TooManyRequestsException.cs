﻿using System;
using JetBrains.Annotations;

namespace Backend.Fx.Exceptions
{
    [PublicAPI]
    public class TooManyRequestsException : ClientException
    {
        public TooManyRequestsException(int retryAfter)
            : base("TooManyRequests")
        {
            RetryAfter = retryAfter;
        }

        /// <inheritdoc />
        public TooManyRequestsException(int retryAfter, string message) : base(message)
        {
            RetryAfter = retryAfter;
        }

        /// <inheritdoc />
        public TooManyRequestsException(int retryAfter, string message, Exception innerException) : base(message, innerException)
        {
            RetryAfter = retryAfter;
        }

        public int RetryAfter { get; }
    }
}