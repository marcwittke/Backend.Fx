namespace Backend.Fx.Exceptions
{
    using System;

    public class ForbiddenException : ClientException
    {
        public enum Code
        {
            AccessDenied,
            NotAuthenticated,
            AccountLocked,
            InvalidLogonAttempt,
        }

        public ForbiddenException() 
                : base("Unauthorized")
        { }

        public ForbiddenException(params Error[] errors) 
                : base("Unauthorized", errors)
        { }

        public ForbiddenException(string message, params Error[] errors) 
                : base(message, errors)
        { }

        public ForbiddenException(string message, Exception innerException, params Error[] errors)
                : base(message, innerException, errors)
        { }
        
        public static IExceptionBuilder UseBuilder()
        {
            return new ExceptionBuilder<ForbiddenException>();
        }
    }
}
