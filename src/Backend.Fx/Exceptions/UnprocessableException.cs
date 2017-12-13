namespace Backend.Fx.Exceptions
{
    using System;

    public class UnprocessableException : ClientException
    {
        public UnprocessableException()
        { }

        public UnprocessableException(string message) : base(message)
        { }

        public UnprocessableException(string message, string errorCode) : base(message)
        {
            ErrorCode = errorCode;
        }

        public UnprocessableException(string message, Exception innerException) : base(message, innerException)
        { }

        public UnprocessableException(string message, string errorCode, Exception innerException) : base(message, innerException)
        {
            ErrorCode = errorCode;
        }

        public static UnprocessableExceptionBuilder UseBuilder()
        {
            return new UnprocessableExceptionBuilder();
        }

        public string ErrorCode { get; }
    }
}