using System;

namespace Backend.Fx.Exceptions
{
    public class ConflictedException : ClientException
    {
        public ConflictedException()
            : base("Conflicted")
        {
        }

        public ConflictedException(string message)
            : base(message)
        {
        }

        public ConflictedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public static IExceptionBuilder UseBuilder()
        {
            return new ExceptionBuilder<ConflictedException>();
        }
    }
}