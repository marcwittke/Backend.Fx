namespace Backend.Fx.Exceptions
{
    using System;

    public class UnauthorizedException : ClientException
    {
        public enum Code
        {
            AccessDenied,
            NotAuthenticated,
            AccountLocked
        }

        public UnauthorizedException()
        { }

        public UnauthorizedException(string message) : base(message)
        { }

        public UnauthorizedException(string message, Exception innerException) : base(message, innerException)
        { }

        public static UnauthorizedException AccessDenied()
        {
            var exception = new UnauthorizedException();
            exception.Errors.Add(new Error(Code.AccessDenied, "Access to this function or data is denied."));
            return exception;
        }

        public static UnauthorizedException AccountLocked()
        {
            var exception = new UnauthorizedException();
            exception.Errors.Add(new Error(Code.AccountLocked, "The identity's user account is locked."));
            return exception;
        }

        public static UnauthorizedException NotAuthenticated()
        {
            var exception = new UnauthorizedException();
            exception.Errors.Add(new Error(Code.NotAuthenticated, "Identity must be authenticated to access this function or data."));
            return exception;
        }

        protected override string DefaultMessage
        {
            get { return "Unauthorized access."; }
        }
    }
}
