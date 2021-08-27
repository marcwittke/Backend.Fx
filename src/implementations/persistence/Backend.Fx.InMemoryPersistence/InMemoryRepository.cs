﻿using System.Linq;
using Backend.Fx.BuildingBlocks;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Extensions;
using Backend.Fx.Patterns.Authorization;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.RandomData;

namespace Backend.Fx.InMemoryPersistence
{
    public class InMemoryRepository<T> : QueryableRepository<T> where T : AggregateRoot
    {
        public InMemoryRepository(InMemoryStore<T> store, ICurrentTHolder<TenantId> currentTenantIdHolder, IAggregateAuthorization<T> aggregateAuthorization)
            : base(currentTenantIdHolder, aggregateAuthorization)
        {
            Store = store;
        }

        public virtual InMemoryStore<T> Store { get; }

        protected override IQueryable<T> RawAggregateQueryable => Store.Values.AsQueryable();

        public void Clear()
        {
            Store.Clear();
        }

        public T Random()
        {
            return Store.Values.Random();
        }
        

        protected override void AddPersistent(T aggregateRoot)
        {
            Store.Add(aggregateRoot.Id, aggregateRoot);
        }

        protected override void AddRangePersistent(T[] aggregateRoots)
        {
            aggregateRoots.ForAll(AddPersistent);
        }

        protected override void DeletePersistent(T aggregateRoot)
        {
            Store.Remove(aggregateRoot.Id);
        }
    }
}