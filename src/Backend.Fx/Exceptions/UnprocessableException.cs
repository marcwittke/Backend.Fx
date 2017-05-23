namespace Backend.Fx.Exceptions
{
    using System;

    public class UnprocessableException : ClientException
    {
        public UnprocessableException()
        { }

        public UnprocessableException(string message) : base(message)
        { }

        public UnprocessableException(string message, Exception innerException) : base(message, innerException)
        { }

        public static UnprocessableExceptionBuilder UseBuilder()
        {
            return new UnprocessableExceptionBuilder();
        }
    }

}