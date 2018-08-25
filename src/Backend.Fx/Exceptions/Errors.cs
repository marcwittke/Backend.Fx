namespace Backend.Fx.Exceptions
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class Errors : IReadOnlyDictionary<string, Error[]>
    {
        public const string GenericErrorKey = "_error";
        private readonly IDictionary<string, List<Error>> dictionaryImplementation = new Dictionary<string, List<Error>>();

        public bool ContainsKey(string key)
        {
            return dictionaryImplementation.ContainsKey(key);
        }

        public bool TryGetValue(string key, out Error[] value)
        {
            if (dictionaryImplementation.TryGetValue(key, out List<Error> errors))
            {
                value = errors.ToArray();
                return true;
            }

            value = null;
            return false;
        }

        public Error[] this[string key]
        {
            get { return dictionaryImplementation[key].ToArray(); }
        }

        public IEnumerable<string> Keys
        {
            get { return dictionaryImplementation.Keys; }
        }

        public IEnumerable<Error[]> Values
        {
            get { return dictionaryImplementation.Values.Select(errors => errors.ToArray()); }
        }

        public void Add(string key, IEnumerable<Error> errors)
        {
            if (!dictionaryImplementation.ContainsKey(key))
            {
                dictionaryImplementation[key] = new List<Error>(errors);
            }
            else
            {
                dictionaryImplementation[key].AddRange(errors);
            }
        }

        public void Add(string key, Error error)
        {
            if (!dictionaryImplementation.ContainsKey(key))
            {
                dictionaryImplementation[key] = new List<Error>();
            }

            dictionaryImplementation[key].Add(error);
        }

        public void Add(Error error)
        {
            Add(GenericErrorKey, error);
        }

        public void Add(IEnumerable<Error> errors)
        {
            var errorArray = errors as Error[] ?? errors.ToArray();
            if (errorArray.Any()) 
            {
                Add(GenericErrorKey, errorArray);
            }
        }

        public IEnumerator<KeyValuePair<string, Error[]>> GetEnumerator()
        {
            return dictionaryImplementation.Select(kvp => new KeyValuePair<string, Error[]>(kvp.Key, kvp.Value.ToArray())).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count
        {
            get { return dictionaryImplementation.Count; }
        }

        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            b.Append("Errors: ");
            b.Append(Count.ToString());
            b.AppendLine();

            foreach (var keyValuePair in this)
            {
                b.Append("  ");
                b.Append(keyValuePair.Key);
                b.AppendLine();
                for (var index = 0; index < keyValuePair.Value.Length; index++)
                {
                    var error = keyValuePair.Value[index];
                    string indexString = $"[{index}]".PadLeft(4);
                    b.Append("    ");
                    b.Append(indexString);
                    b.Append(" ");
                    b.Append(error.Code);
                    b.Append(":");
                    b.Append(error.Message);
                    b.AppendLine();
                }
            }

            return b.ToString();
        }
    }
}