namespace Backend.Fx.Exceptions
{
    using System;

    public class ConflictedException : ClientException
    {
        public ConflictedException()
                : base("Conflicted.")
        { }

        public ConflictedException(params Error[] errors)
                : base("Conflicted", errors)
        { }

        public ConflictedException(string message, params Error[] errors)
                : base(message, errors)
        { }

        public ConflictedException(string message, Exception innerException, params Error[] errors)
                : base(message, innerException, errors)
        { }

        public static IExceptionBuilder UseBuilder()
        {
            return new ExceptionBuilder<ConflictedException>();
        }
    }
}