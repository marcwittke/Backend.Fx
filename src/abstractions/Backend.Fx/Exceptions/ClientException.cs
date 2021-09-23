using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Backend.Fx.Exceptions
{
    public class ClientException : Exception
    {
        public ClientException()
            : base("Bad request.")
        { }

        /// <param name="message">
        /// When using one of the middlewares in Backend.Fx.AspNetCore.ErrorHandling, the message is not sent
        /// to the client to not provide internal details to an attacker. Write the exception message with a developer in mind,
        /// since
        /// the application log will contain the message. To provide the user with functional feedback to correct their input, use
        /// the AddError(s) overloads.
        /// </param>
        public ClientException(string message)
            : base(message)
        { }

        /// <param name="message">
        /// When using one of the middlewares in Backend.Fx.AspNetCore.ErrorHandling, the message is not sent
        /// to the client to not provide internal details to an attacker. Write the exception message with a developer in mind,
        /// since
        /// the application log will contain the message. To provide the user with functional feedback to correct their input, use
        /// the AddError(s) overloads.
        /// </param>
        /// <param name="innerException"></param>
        public ClientException(string message, Exception innerException)
            : base(message, innerException)
        { }

        public Errors Errors { get; } = new Errors();

        public bool HasErrors()
        {
            return Errors.Any();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var exceptionType = GetType().ToString();

            string message = string.IsNullOrEmpty(Message)
                ? exceptionType
                : exceptionType + ": " + Message;

            string innerException = InnerException != null
                ? " ---> "
                + InnerException
                + System.Environment.NewLine
                + "   End of inner exception stack trace"
                : null;

            return string.Join(
                System.Environment.NewLine,
                new[] { message, Errors.ToString(), innerException, StackTrace }.Where(s => s != null));
        }

        /// <summary>
        /// Used to build an <see cref="ClientException" /> with multiple possible error messages. The builder will throw on
        /// disposal
        /// when at least one error was added. Using the AddIf methods is quite comfortable when there are several criteria to be
        /// validated
        /// before executing a business case.
        /// </summary>
        public static IExceptionBuilder UseBuilder()
        {
            return new ExceptionBuilder<ClientException>();
        }
    }


    public static class ClientExceptionEx
    {
        public static TEx AddError<TEx>(this TEx clientException, [LocalizationRequired] string errorMessage)
            where TEx : ClientException
        {
            clientException.Errors.Add(errorMessage);
            return clientException;
        }

        public static TEx AddError<TEx>(
            this TEx clientException,
            string key,
            [LocalizationRequired] string errorMessage) where TEx : ClientException
        {
            clientException.Errors.Add(key, errorMessage);
            return clientException;
        }

        public static TEx AddErrors<TEx>(
            this TEx clientException,
            [LocalizationRequired] IEnumerable<string> errorMessages) where TEx : ClientException
        {
            clientException.Errors.Add(errorMessages);
            return clientException;
        }

        public static TEx AddErrors<TEx>(
            this TEx clientException,
            string key,
            [LocalizationRequired] IEnumerable<string> errorMessages) where TEx : ClientException
        {
            clientException.Errors.Add(key, errorMessages);
            return clientException;
        }
    }
}
