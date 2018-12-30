namespace Backend.Fx.Exceptions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;


    public class ClientException : Exception
    {
        public ClientException()
                : base("Bad request.")
        { }

        public ClientException(string message)
                : base(message)
        { }

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
            string exceptionType = GetType().ToString();

            string message = string.IsNullOrEmpty(Message)
                                     ? exceptionType
                                     : exceptionType + ": " + Message;

            string innerException = InnerException != null
                                            ? " ---> "
                                              + InnerException
                                              + Environment.NewLine
                                              + "   End of inner exception stack trace"
                                            : null;

            return string.Join(Environment.NewLine,
                               new[] { message, Errors.ToString(), innerException, StackTrace }.Where(s => s != null));
        }
    }

    public static class ClientExceptionEx
    {
        public static TEx AddError<TEx>(this TEx clientException, [LocalizationRequired] string errorMessage) where TEx : ClientException
        {
            clientException.Errors.Add(errorMessage);
            return clientException;
        }

        public static TEx AddError<TEx>(this TEx clientException, string key, [LocalizationRequired] string errorMessage) where TEx : ClientException
        {
            clientException.Errors.Add(key, errorMessage);
            return clientException;
        }

        public static TEx AddErrors<TEx>(this TEx clientException, [LocalizationRequired] IEnumerable<string> errorMessages) where TEx : ClientException
        {
            clientException.Errors.Add(errorMessages);
            return clientException;
        }


        public static TEx AddErrors<TEx>(this TEx clientException, string key, [LocalizationRequired] IEnumerable<string> errorMessages) where TEx : ClientException
        {
            clientException.Errors.Add(key, errorMessages);
            return clientException;
        }
    }
}
