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
        IAggregateQueryable<TAggregateRoot, TId> GetQueryable<TAggregateRoot, TId>()
            where TAggregateRoot : class, IAggregateRoot<TId>
            where TId : IEquatable<TId>;
    }
    

    public class AggregateDictionaries : IAggregateDictionaries
    {
        private readonly ConcurrentDictionary<Type, object> _aggregateDictionaries = new();

        public AggregateDictionary<TAggregateRoot, TId> For<TAggregateRoot, TId>() 
            where TAggregateRoot : IAggregateRoot<TId>
            where TId : IEquatable<TId>
        {
            var store = (AggregateDictionary<TAggregateRoot, TId>)_aggregateDictionaries.GetOrAdd(
                typeof(TAggregateRoot),
                _ => new AggregateDictionary<TAggregateRoot, TId>());
            return store;
        }

        public IAggregateQueryable<TAggregateRoot, TId> GetQueryable<TAggregateRoot, TId>()
            where TAggregateRoot : class, IAggregateRoot<TId>
            where TId : IEquatable<TId>
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