using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Backend.Fx.BuildingBlocks;
using Backend.Fx.ConfigurationSettings;
using Backend.Fx.Environment.Authentication;
using Backend.Fx.Environment.DateAndTime;
using Backend.Fx.InMemoryPersistence;
using Backend.Fx.Patterns.EventAggregation.Domain;
using Backend.Fx.Patterns.EventAggregation.Integration;
using Backend.Fx.Patterns.IdGeneration;
using Backend.Fx.Patterns.UnitOfWork;
using Backend.Fx.SimpleInjectorDependencyInjection.Modules;
using FakeItEasy;
using SimpleInjector;

namespace Backend.Fx.SimpleInjectorDependencyInjection.Tests.DummyImpl.Bootstrapping
{
    public class APersistenceModule : SimpleInjectorModule
    {
        public Dictionary<Type, object> Stores { get; }

        public APersistenceModule(params Assembly[] domainAssemblies)
        {
            Stores = domainAssemblies.SelectMany(ass => ass.GetExportedTypes())
                    .Where(t => !t.GetTypeInfo().IsAbstract && t.GetTypeInfo().IsClass)
                    .Where(t => typeof(AggregateRoot).IsAssignableFrom(t))
                    .Select(t => {
                                var storeType = typeof(InMemoryStore<>).MakeGenericType(t);
                                var store = Activator.CreateInstance(storeType);
                                return new { t, store };
                            })
                    .ToDictionary(arg => arg.t, arg => arg.store);

            // the aggregate root "setting" resides outside the scanned assembly. Adding a repo manually now.
            Stores.Add(typeof(Setting), new InMemoryStore<Setting>());
        }

        protected override void Register(Container container, ScopedLifestyle scopedLifestyle)
        {
            foreach (var store in Stores)
            {
                container.RegisterSingleton(typeof(IInMemoryStore<>).MakeGenericType(store.Key), () => store.Value);
            }

            container.Register(typeof(IRepository<>), typeof(InMemoryRepository<>));
            container.Register(typeof(IQueryable<>), typeof(InMemoryQueryable<>));

            var uowRegistration = Lifestyle.Scoped.CreateRegistration(
                    ()  => new InMemoryUnitOfWork(new FrozenClock(), CurrentIdentityHolder.CreateSystem(), 
                                                  container.GetInstance<IDomainEventAggregator>(), 
                                                  container.GetInstance<IEventBusScope>()), 
                    container);
            container.AddRegistration(typeof(IUnitOfWork), uowRegistration);
            container.AddRegistration(typeof(ICanFlush), uowRegistration);
            container.Register(A.Fake<IReadonlyUnitOfWork>);
            container.Register(A.Fake<ICanInterruptTransaction>);

            container.RegisterInstance<IEntityIdGenerator>(new InMemoryEntityIdGenerator());
        }
    }
}
