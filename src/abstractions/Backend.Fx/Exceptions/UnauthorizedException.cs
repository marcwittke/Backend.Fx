using System;

namespace Backend.Fx.Exceptions
{
    public class UnauthorizedException : ClientException
    {
        public UnauthorizedException()
            : base("Unauthorized")
        { }
        
        public UnauthorizedException(string message)
            : base(message)
        { }

        public UnauthorizedException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}