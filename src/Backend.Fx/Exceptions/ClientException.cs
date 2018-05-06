namespace Backend.Fx.Exceptions
{
    using System;
    using System.Linq;

    public class ClientException : Exception
    {
        public ClientException()
                : base("Bad request.")
        {}

        public ClientException(params Error[] errors) 
                : base("Bad request.")
        {
            Errors.Add(errors);
        }

        public ClientException(string message, params Error[] errors) 
                : base(message)
        {
            Errors.Add(errors);
        }

        public ClientException(string message, Exception innerException, params Error[] errors) 
                : base(message, innerException)
        {
            Errors.Add(errors);
        }

        public Errors Errors { get; } = new Errors();

        public bool HasErrors()
        {
            return Errors.Any();
        }
    }
}
