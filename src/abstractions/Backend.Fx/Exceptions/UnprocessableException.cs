namespace Backend.Fx.Exceptions
{
    using System;

    public class UnprocessableException : ClientException
    {
        public UnprocessableException() 
                : base("The provided arguments could not be processed.")
        {}

        public UnprocessableException(params Error[] errors) 
                : base("The provided arguments could not be processed.", errors)
        { }

        public UnprocessableException(string message, params Error[] errors) 
                : base(message, errors)
        { }

        public UnprocessableException(string message, Exception innerException, params Error[] errors) 
                : base(message, innerException, errors)
        { }

        public static IExceptionBuilder UseBuilder()
        {
            return new ExceptionBuilder<UnprocessableException>();
        }
    }
}