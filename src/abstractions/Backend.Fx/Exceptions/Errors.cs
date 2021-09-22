using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Backend.Fx.Exceptions
{
    public class Errors : IReadOnlyDictionary<string, string[]>
    {
        private const string GenericErrorKey = "";

        private readonly IDictionary<string, List<string>> _dictionaryImplementation
            = new Dictionary<string, List<string>>();

        public bool ContainsKey(string key)
        {
            return _dictionaryImplementation.ContainsKey(key);
        }

        public bool TryGetValue(string key, out string[] value)
        {
            if (_dictionaryImplementation.TryGetValue(key, out List<string> errors))
            {
                value = errors.ToArray();
                return true;
            }

            value = null;
            return false;
        }

        public string[] this[string key] => _dictionaryImplementation[key].ToArray();

        public IEnumerable<string> Keys => _dictionaryImplementation.Keys;

        public IEnumerable<string[]> Values
        {
            get { return _dictionaryImplementation.Values.Select(errors => errors.ToArray()); }
        }

        public IEnumerator<KeyValuePair<string, string[]>> GetEnumerator()
        {
            return _dictionaryImplementation
                .Select(kvp => new KeyValuePair<string, string[]>(kvp.Key, kvp.Value.ToArray()))
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => _dictionaryImplementation.Count;

        public Errors Add(string errorMessage)
        {
            Add(GenericErrorKey, errorMessage);
            return this;
        }

        public Errors Add(IEnumerable<string> errorMessages)
        {
            Add(GenericErrorKey, errorMessages);
            return this;
        }

        public Errors Add(string key, IEnumerable<string> errorMessages)
        {
            if (!_dictionaryImplementation.ContainsKey(key))
            {
                _dictionaryImplementation[key] = new List<string>(errorMessages);
            }
            else
            {
                _dictionaryImplementation[key].AddRange(errorMessages);
            }

            return this;
        }

        public Errors Add(string key, string error)
        {
            if (!_dictionaryImplementation.ContainsKey(key))
            {
                _dictionaryImplementation[key] = new List<string>();
            }

            _dictionaryImplementation[key].Add(error);

            return this;
        }

        public override string ToString()
        {
            var b = new StringBuilder();
            b.Append("Errors: ");
            b.Append(Count.ToString());
            b.AppendLine();

            foreach (KeyValuePair<string, string[]> keyValuePair in this)
            {
                b.Append("  ");
                b.Append(string.IsNullOrEmpty(keyValuePair.Key) ? "(generic)" : keyValuePair.Key);
                b.AppendLine();
                for (var index = 0; index < keyValuePair.Value.Length; index++)
                {
                    string error = keyValuePair.Value[index];
                    b.Append("    ");
                    b.Append($"[{index}]".PadLeft(4));
                    b.Append(" ");
                    b.Append(error);
                    b.AppendLine();
                }
            }

            return b.ToString();
        }
    }
}
