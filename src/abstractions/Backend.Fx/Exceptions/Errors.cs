using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace Backend.Fx.Exceptions
{
    /// <summary>
    /// A structure to collect general or key related errors on a <see cref="ClientException"/>
    /// </summary>
    [PublicAPI]
    public class Errors : IReadOnlyDictionary<string, string[]>
    {
        public const string GenericErrorKey = "";

        private readonly IDictionary<string, List<string>> _dictionaryImplementation =
            new Dictionary<string, List<string>>();

        public Errors()
        { }

        public Errors(IDictionary<string, string[]> dictionary) : this(null, dictionary)
        { }
        
        public Errors(string genericError, IDictionary<string, string[]> dictionary = null)
        {
            if (genericError != null)
            {
                Add(genericError);
            }
            
            if (dictionary != null)
            {
                foreach (var kvp in dictionary)
                {
                    Add(kvp.Key, kvp.Value);
                }
            }
        }
        
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

        internal Errors Add(string errorMessage)
        {
            Add(GenericErrorKey, errorMessage);
            return this;
        }

        internal Errors Add(IEnumerable<string> errorMessages)
        {
            Add(GenericErrorKey, errorMessages);
            return this;
        }

        internal Errors Add(string key, IEnumerable<string> errorMessages)
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

        internal Errors Add(string key, string error)
        {
            if (!_dictionaryImplementation.ContainsKey(key))
            {
                _dictionaryImplementation[key] = new List<string>();
            }

            _dictionaryImplementation[key].Add(error);

            return this;
        }

        public IEnumerator<KeyValuePair<string, string[]>> GetEnumerator()
        {
            return _dictionaryImplementation
                .Select(kvp => new KeyValuePair<string, string[]>(kvp.Key, kvp.Value.ToArray())).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => _dictionaryImplementation.Count;

        public override string ToString()
        {
            var b = new StringBuilder();
            b.Append("Errors: ");
            b.Append(Count.ToString());
            b.AppendLine();

            foreach (var keyValuePair in this)
            {
                b.Append("  ");
                b.Append(keyValuePair.Key == GenericErrorKey ? "(generic)": keyValuePair.Key);
                b.AppendLine();
                for (var index = 0; index < keyValuePair.Value.Length; index++)
                {
                    var error = keyValuePair.Value[index];
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