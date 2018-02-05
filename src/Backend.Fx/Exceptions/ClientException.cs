namespace Backend.Fx.Exceptions
{
    using System;
    using System.Linq;

    public class ClientException : Exception
    {
        public ClientException()
        {}

        public ClientException(string message) : base(message)
        {}

        public ClientException(string message, Exception innerException) : base(message, innerException)
        {}

        public Errors Errors { get; } = new Errors();

        public bool HasErrors()
        {
            return Errors.Any();
        }
    }
}
