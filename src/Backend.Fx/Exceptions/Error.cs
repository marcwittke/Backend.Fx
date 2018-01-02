namespace Backend.Fx.Exceptions
{
    public class Error
    {
        public const string GenericKey = "_error";

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