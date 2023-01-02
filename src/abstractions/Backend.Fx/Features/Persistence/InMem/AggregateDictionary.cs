using System;
using System.Collections;
using System.Collections.Generic;
using Backend.Fx.Domain;
using JetBrains.Annotations;

namespace Backend.Fx.Features.Persistence.InMem
{
    public interface IAggregateDictionary<TAggregateRoot, TId> : IDictionary<TId, TAggregateRoot> where TAggregateRoot : IAggregateRoot<TId>
        where TId : IEquatable<TId>
    { }

    [UsedImplicitly]
    public class AggregateDictionary<TAggregateRoot, TId> : IAggregateDictionary<TAggregateRoot, TId> 
        where TAggregateRoot : IAggregateRoot<TId>
        where TId : IEquatable<TId>
    {
        private readonly IDictionary<TId, TAggregateRoot> _dictionary = new Dictionary<TId, TAggregateRoot>();

        public IEnumerator<KeyValuePair<TId, TAggregateRoot>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _dictionary).GetEnumerator();
        }

        public void Add(KeyValuePair<TId, TAggregateRoot> item)
        {
            _dictionary.Add(item);
        }

        public void Clear()
        {
            _dictionary.Clear();
        }

        public bool Contains(KeyValuePair<TId, TAggregateRoot> item)
        {
            return _dictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<TId, TAggregateRoot>[] array, int arrayIndex)
        {
            _dictionary.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TId, TAggregateRoot> item)
        {
            return _dictionary.Remove(item);
        }

        public int Count => _dictionary.Count;

        public bool IsReadOnly => _dictionary.IsReadOnly;

        public void Add(TId key, TAggregateRoot value)
        {
            _dictionary.Add(key, value);
        }

        public bool ContainsKey(TId key)
        {
            return _dictionary.ContainsKey(key);
        }

        public bool Remove(TId key)
        {
            return _dictionary.Remove(key);
        }

        public bool TryGetValue(TId key, out TAggregateRoot value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        public TAggregateRoot this[TId key]
        {
            get => _dictionary[key];
            set => _dictionary[key] = value;
        }

        public ICollection<TId> Keys => _dictionary.Keys;

        public ICollection<TAggregateRoot> Values => _dictionary.Values;
    }
}