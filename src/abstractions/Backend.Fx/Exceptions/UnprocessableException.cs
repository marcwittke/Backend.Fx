using System;

namespace Backend.Fx.Exceptions
{
    public class UnprocessableException : ClientException
    {
        public UnprocessableException()
            : base("The provided arguments could not be processed.")
        { }

        /// <inheritdoc />
        public UnprocessableException(string message)
            : base(message)
        { }

        /// <inheritdoc />
        public UnprocessableException(string message, Exception innerException)
            : base(message, innerException)
        { }

        /// <summary>
        /// Used to build an <see cref="UnprocessableException" /> with multiple possible error messages. The builder will throw on
        /// disposal
        /// when at least one error was added. Using the AddIf methods is quite comfortable when there are several criteria to be
        /// validated
        /// before executing a business case.
        /// </summary>
        public new static IExceptionBuilder UseBuilder()
        {
            return new ExceptionBuilder<UnprocessableException>();
        }
    }
}
