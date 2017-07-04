namespace Backend.Fx.Exceptions
{
    using System;

    public class ConflictedException : ClientException
    {
        public ConflictedException()
        { }

        public ConflictedException(string message) : base(message)
        { }

        public ConflictedException(string message, Exception innerException) : base(message, innerException)
        { }
    }
}