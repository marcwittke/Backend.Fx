using System;

namespace Backend.Fx.Exceptions
{
    public class ConflictedException : ClientException
    {
        public ConflictedException()
            : base("Conflicted")
        {
        }

        /// <inheritdoc />
        public ConflictedException(string message)
            : base(message)
        {
        }

        /// <inheritdoc />
        public ConflictedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}