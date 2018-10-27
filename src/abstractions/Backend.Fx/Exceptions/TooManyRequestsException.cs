using System;

namespace Backend.Fx.Exceptions
{
    public class TooManyRequestsException : ClientException
    {
        public TooManyRequestsException()
        {
        }

        public TooManyRequestsException(params Error[] errors) : base(errors)
        {
        }

        public TooManyRequestsException(string message, params Error[] errors) : base(message, errors)
        {
        }

        public TooManyRequestsException(string message, Exception innerException, params Error[] errors) : base(message, innerException, errors)
        {
        }
    }
}
