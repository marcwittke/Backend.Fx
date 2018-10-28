using System;

namespace Backend.Fx.Exceptions
{
    public class UnauthorizedException : ClientException
    {
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
    }
}