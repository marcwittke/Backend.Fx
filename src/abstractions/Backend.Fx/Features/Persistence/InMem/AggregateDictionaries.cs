using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Backend.Fx.Domain;

namespace Backend.Fx.Features.Persistence.InMem
{
    public interface IAggregateDictionaries
    {
        IQueryable<TAggregateRoot> GetQueryable<TAggregateRoot>()
            where TAggregateRoot : IAggregateRoot;
    }
    
    public interface IAggregateDictionaries<TId> : IAggregateDictionaries
        where TId : IEquatable<TId>
    {
        AggregateDictionary<TAggregateRoot, TId> For<TAggregateRoot>()
            where TAggregateRoot : IAggregateRoot<TId>;
    }

    public class AggregateDictionaries<TId> : IAggregateDictionaries<TId> where TId : IEquatable<TId>
    {
        private readonly ConcurrentDictionary<Type, object> _aggregateDictionaries = new();

        public AggregateDictionary<TAggregateRoot, TId> For<TAggregateRoot>() 
            where TAggregateRoot : IAggregateRoot<TId>
        {
            var store = (AggregateDictionary<TAggregateRoot, TId>)_aggregateDictionaries.GetOrAdd(
                typeof(TAggregateRoot),
                _ => new AggregateDictionary<TAggregateRoot, TId>());
            return store;
        }

        public IQueryable<TAggregateRoot> GetQueryable<TAggregateRoot>() where TAggregateRoot : IAggregateRoot
        {
            dynamic store = _aggregateDictionaries.GetOrAdd(
                typeof(TAggregateRoot),
                aggType =>
                {
                    PropertyInfo idPropertyInfo = aggType.GetProperty("Id") ?? aggType.GetProperty("ID");
                    Debug.Assert(idPropertyInfo != null);
                    Type idType = idPropertyInfo.PropertyType;
                    Type aggregateDictionaryType = typeof(AggregateDictionary<,>).MakeGenericType(aggType, idType);
                    return Activator.CreateInstance(aggregateDictionaryType);
                });
            return Queryable.AsQueryable(store.Values);
            
            
        }
    }
}