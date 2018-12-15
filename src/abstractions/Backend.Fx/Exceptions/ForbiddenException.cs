namespace Backend.Fx.Exceptions
{
    using System;

    public class ForbiddenException : ClientException
    {
        public ForbiddenException() 
                : base("Unauthorized")
        { }

        public ForbiddenException(string message) 
                : base(message)
        { }

        public ForbiddenException(string message, Exception innerException)
                : base(message, innerException)
        { }
        
        public static IExceptionBuilder UseBuilder()
        {
            return new ExceptionBuilder<ForbiddenException>();
        }
    }
}
