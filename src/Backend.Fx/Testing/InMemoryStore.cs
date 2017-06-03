namespace Backend.Fx.Testing
{
    using System.Collections;
    using System.Collections.Generic;
    using BuildingBlocks;

    public interface IInMemoryStore<T> : IDictionary<int, T> where T : AggregateRoot
    { }
    
    public class InMemoryStore<T> : IInMemoryStore<T> where T:AggregateRoot
    {
        private readonly IDictionary<int, T> dictionaryImplementation = new Dictionary<int, T>();

        public IEnumerator<KeyValuePair<int, T>> GetEnumerator()
        {
            return dictionaryImplementation.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) dictionaryImplementation).GetEnumerator();
        }

        public void Add(KeyValuePair<int, T> item)
        {
            dictionaryImplementation.Add(item);
        }

        public void Clear()
        {
            dictionaryImplementation.Clear();
        }

        public bool Contains(KeyValuePair<int, T> item)
        {
            return dictionaryImplementation.Contains(item);
        }

        public void CopyTo(KeyValuePair<int, T>[] array, int arrayIndex)
        {
            dictionaryImplementation.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<int, T> item)
        {
            return dictionaryImplementation.Remove(item);
        }

        public int Count
        {
            get { return dictionaryImplementation.Count; }
        }

        public bool IsReadOnly
        {
            get { return dictionaryImplementation.IsReadOnly; }
        }

        public void Add(int key, T value)
        {
            dictionaryImplementation.Add(key, value);
        }

        public bool ContainsKey(int key)
        {
            return dictionaryImplementation.ContainsKey(key);
        }

        public bool Remove(int key)
        {
            return dictionaryImplementation.Remove(key);
        }

        public bool TryGetValue(int key, out T value)
        {
            return dictionaryImplementation.TryGetValue(key, out value);
        }

        public T this[int key]
        {
            get { return dictionaryImplementation[key]; }
            set { dictionaryImplementation[key] = value; }
        }

        public ICollection<int> Keys
        {
            get { return dictionaryImplementation.Keys; }
        }

        public ICollection<T> Values
        {
            get { return dictionaryImplementation.Values; }
        }
    }
}