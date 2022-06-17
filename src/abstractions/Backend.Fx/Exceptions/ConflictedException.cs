using System;
using JetBrains.Annotations;

namespace Backend.Fx.Exceptions
{
    [PublicAPI]
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