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

        public override string Message
        {
            get 
            { 
                if (!string.IsNullOrEmpty(base.Message))
                {
                    return base.Message;
                }

                if (HasErrors() && Errors.TryGetValue(Errors.GenericErrorKey, out Error[] genericErrors))
                {
                    var errors = genericErrors.Select(err => $"{err.Code}:{err.Message}");
                    return string.Join(". ", errors);
                }

                return DefaultMessage;
            }       
        }

        protected virtual string DefaultMessage
        {
            get { return "Bad Request."; }
        }
    }
}
