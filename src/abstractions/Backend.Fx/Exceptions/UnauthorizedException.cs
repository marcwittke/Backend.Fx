using System;

namespace Backend.Fx.Exceptions
{
    public class UnauthorizedException : ClientException
    {
        public UnauthorizedException()
            : base("Unauthorized")
        {
        }

        /// <inheritdoc />
        public UnauthorizedException(string message)
            : base(message)
        {
        }

        /// <inheritdoc />
        public UnauthorizedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}