using System;
using System.Collections.Concurrent;
using Backend.Fx.Domain;

namespace Backend.Fx.Features.Persistence.InMem
{
    public interface IAggregateDictionaries
    {
        AggregateDictionary<TAggregateRoot, TId> Get<TAggregateRoot, TId>() 
            where TAggregateRoot : IAggregateRoot<TId>
            where TId : struct, IEquatable<TId>;
    }

    public class AggregateDictionaries : IAggregateDictionaries
    {
        private readonly ConcurrentDictionary<Type, object> _aggregateDictionaries = new();

        public AggregateDictionary<TAggregateRoot, TId> Get<TAggregateRoot, TId>() 
            where TAggregateRoot : IAggregateRoot<TId>
            where TId : struct, IEquatable<TId>
        {
            var store = (AggregateDictionary<TAggregateRoot, TId>)_aggregateDictionaries.GetOrAdd(typeof(TAggregateRoot),
                _ => new AggregateDictionary<TAggregateRoot, TId>());
            return store;
        }
    }
}