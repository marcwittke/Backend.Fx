﻿using System;

namespace Backend.Fx.Exceptions
{
    public class ForbiddenException : ClientException
    {
        public ForbiddenException()
            : base("Unauthorized")
        {
        }

        /// <inheritdoc />
        public ForbiddenException(string message)
            : base(message)
        {
        }

        /// <inheritdoc />
        public ForbiddenException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}