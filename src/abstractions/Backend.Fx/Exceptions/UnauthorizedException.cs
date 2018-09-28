namespace Backend.Fx.Exceptions
{
    using System;

    public class UnauthorizedException : ClientException
    {
        public enum Code
        {
            AccessDenied,
            NotAuthenticated,
            AccountLocked,
            InvalidLogonAttempt,
        }

        public UnauthorizedException() 
                : base("Unauthorized")
        { }

        public UnauthorizedException(params Error[] errors) 
                : base("Unauthorized", errors)
        { }

        public UnauthorizedException(string message, params Error[] errors) 
                : base(message, errors)
        { }

        public UnauthorizedException(string message, Exception innerException, params Error[] errors)
                : base(message, innerException, errors)
        { }
        
        public static IExceptionBuilder UseBuilder()
        {
            return new ExceptionBuilder<UnauthorizedException>();
        }
    }
}
