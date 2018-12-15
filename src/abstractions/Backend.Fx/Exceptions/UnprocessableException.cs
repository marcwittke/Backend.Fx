namespace Backend.Fx.Exceptions
{
    using System;

    public class UnprocessableException : ClientException
    {
        public UnprocessableException() 
                : base("The provided arguments could not be processed.")
        {}

        public UnprocessableException(string message) 
                : base(message)
        { }

        public UnprocessableException(string message, Exception innerException) 
                : base(message, innerException)
        { }

        public static IExceptionBuilder UseBuilder()
        {
            return new ExceptionBuilder<UnprocessableException>();
        }
    }
}