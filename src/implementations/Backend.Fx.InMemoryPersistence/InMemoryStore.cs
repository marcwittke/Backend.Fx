using System.Collections;
using System.Collections.Generic;
using Backend.Fx.BuildingBlocks;

namespace Backend.Fx.InMemoryPersistence
{
    public interface IInMemoryStore<T> : IDictionary<int, T> where T : AggregateRoot
    { }


    public class InMemoryStore<T> : IInMemoryStore<T> where T : AggregateRoot
    {
        private readonly IDictionary<int, T> _dictionaryImplementation = new Dictionary<int, T>();

        public IEnumerator<KeyValuePair<int, T>> GetEnumerator()
        {
            return _dictionaryImplementation.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_dictionaryImplementation).GetEnumerator();
        }

        public void Add(KeyValuePair<int, T> item)
        {
            _dictionaryImplementation.Add(item);
        }

        public void Clear()
        {
            _dictionaryImplementation.Clear();
        }

        public bool Contains(KeyValuePair<int, T> item)
        {
            return _dictionaryImplementation.Contains(item);
        }

        public void CopyTo(KeyValuePair<int, T>[] array, int arrayIndex)
        {
            _dictionaryImplementation.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<int, T> item)
        {
            return _dictionaryImplementation.Remove(item);
        }

        public int Count => _dictionaryImplementation.Count;

        public bool IsReadOnly => _dictionaryImplementation.IsReadOnly;

        public void Add(int key, T value)
        {
            _dictionaryImplementation.Add(key, value);
        }

        public bool ContainsKey(int key)
        {
            return _dictionaryImplementation.ContainsKey(key);
        }

        public bool Remove(int key)
        {
            return _dictionaryImplementation.Remove(key);
        }

        public bool TryGetValue(int key, out T value)
        {
            return _dictionaryImplementation.TryGetValue(key, out value);
        }

        public T this[int key]
        {
            get => _dictionaryImplementation[key];
            set => _dictionaryImplementation[key] = value;
        }

        public ICollection<int> Keys => _dictionaryImplementation.Keys;

        public ICollection<T> Values => _dictionaryImplementation.Values;
    }
}
