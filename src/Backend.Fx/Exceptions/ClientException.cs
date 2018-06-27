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

        public override string Message
        {
            get { return base.Message + Environment.NewLine + Errors; }
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
                               new [] {message, Errors.ToString(), innerException, StackTrace }.Where(s => s != null));
        }
    }
}
