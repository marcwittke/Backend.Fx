namespace Backend.Fx.Exceptions
{
    public class ErrorEntry
    {
        public ErrorEntry()
        {}

        public ErrorEntry(string key, string[] errorMessages)
        {
            Key = key;
            ErrorMessages = errorMessages;
        }

        public string Key { get; set; }

        public string[] ErrorMessages { get; set; }
    }
}