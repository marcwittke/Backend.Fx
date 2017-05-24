namespace Backend.Fx.Testing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using BuildingBlocks;
    using Environment.MultiTenancy;
    using Exceptions;
    using RandomData;
    
    public class InMemoryRepository<T> : IRepository<T> where T : AggregateRoot
    {
        public TenantId TenantId { get; } = new TenantId(1);
        private int nextId = 1;
        private readonly Dictionary<int, T> store = new Dictionary<int, T>();

        public T Single(int id)
        {
            if (store.ContainsKey(id))
            {
                return store[id];
            }

            throw new NotFoundException<T>(id);
        }

        public T SingleOrDefault(int id)
        {
            return store.ContainsKey(id)
                ? store[id]
                : null;
        }

        public T[] GetAll()
        {
            return store.Values.ToArray();
        }

        public void Delete(int id)
        {
            store.Remove(id);
        }

        public void Delete(T aggregateRoot)
        {
            store.Remove(aggregateRoot.Id);
        }

        public void Add(T aggregateRoot)
        {
            aggregateRoot.Id = nextId++;
            aggregateRoot.TenantId = TenantId.Value;
            store.Add(aggregateRoot.Id, aggregateRoot);
        }

        public bool Any()
        {
            return store.Any();
        }

        public T[] Where(Expression<Func<T, bool>> predicate)
        {
            return store.Values.Where(predicate.Compile()).ToArray();
        }

        public T[] Resolve(IEnumerable<int> ids)
        {
            if (ids == null)
            {
                return new T[0];
            }

            T[] resolved = store.Values.Where(agg => ids.Contains(agg.Id)).ToArray();
            ids = ids as int[] ?? ids.ToArray();
            if (resolved.Length != ids.Count())
            {
                throw new ArgumentException($"The following {typeof(T).Name} ids could not be resolved: {string.Join(", ", ids.Except(resolved.Select(agg => agg.Id)))}");
            }
            return resolved;
        }

        public IQueryable<T> AggregateQueryable
        {
            get { return store.Values.AsQueryable(); }
        }

        public T Random()
        {
            return store.Values.Random();
        }

        public void Clear()
        {
            store.Clear();
        }
    }
}
