using System;
using System.Collections.Concurrent;
using Backend.Fx.BuildingBlocks;

namespace Backend.Fx.InMemoryPersistence
{
    public interface IInMemoryStores
    {
        InMemoryStore<T> GetStore<T>() where T : AggregateRoot;
    }


    public class InMemoryStores : IInMemoryStores
    {
        private readonly ConcurrentDictionary<Type, object> _dictionaries = new();

        public InMemoryStore<T> GetStore<T>() where T : AggregateRoot
        {
            return (InMemoryStore<T>)_dictionaries.GetOrAdd(typeof(T), _ => new InMemoryStore<T>());
        }
    }
}
