namespace Backend.Fx.Exceptions
{
    public class Error
    {
        public Error(string message)
        {
            Code = "";
            Message = message;
        }

        public Error(object code, string message)
        {
            Code = code.ToString();
            Message = message;
        }

        public Error(string code, string message)
        {
            Code = code;
            Message = message;
        }

        public string Code { get; }
        public string Message { get; }
    }
}