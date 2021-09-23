using System.Reflection;
using Backend.Fx.Patterns.DependencyInjection.Pure;
using Backend.Fx.Patterns.IdGeneration;

namespace Backend.Fx.InMemoryPersistence
{
    public abstract class InMemorySingletonServices<TInMemoryScopedServices>
        : SingletonServices<TInMemoryScopedServices>
        where TInMemoryScopedServices : IScopedServices
    {
        protected InMemorySingletonServices(params Assembly[] assemblies) : base(assemblies)
        {
            Stores = new InMemoryStores();
        }

        public InMemoryStores Stores { get; }

        public override IEntityIdGenerator EntityIdGenerator { get; } = new InMemoryEntityIdGenerator();
    }
}
