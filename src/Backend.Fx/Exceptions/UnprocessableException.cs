namespace Backend.Fx.Exceptions
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    public class UnprocessableException : ClientException
    {
        private ConcurrentDictionary<string, List<string>> errors = new ConcurrentDictionary<string, List<string>>();

        public UnprocessableException()
        { }

        public UnprocessableException(string message) : base(message)
        { }

        public UnprocessableException(string message, Exception innerException) : base(message, innerException)
        { }

        public ErrorEntry[] Errors
        {
            get { return errors.Select(err => new ErrorEntry(err.Key, err.Value.ToArray())).ToArray(); }
            set
            {
                value = value ?? new ErrorEntry[0];
                var dict = value
                           .GroupBy(e => e.Key)
                           .Select(e => new KeyValuePair<string, List<string>>(
                                           e.Key,
                                           e.SelectMany(ee => ee.ErrorMessages).ToList()));
                errors = new ConcurrentDictionary<string, List<string>>(dict);
            }
        }

        internal bool HasErrors
        {
            get { return errors.Any(); }
        }

        public static UnprocessableExceptionBuilder UseBuilder()
        {
            return new UnprocessableExceptionBuilder();
        }

        public void AddError(string key, string errorMessage)
        {
            errors.GetOrAdd(key, s => new List<string>()).Add(errorMessage);
        }
    }
}