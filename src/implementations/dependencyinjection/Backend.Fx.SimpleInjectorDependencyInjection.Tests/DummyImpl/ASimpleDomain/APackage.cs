using System.Linq;
using Backend.Fx.BuildingBlocks;
using Backend.Fx.InMemoryPersistence;
using SimpleInjector;
using SimpleInjector.Packaging;

namespace Backend.Fx.SimpleInjectorDependencyInjection.Tests.DummyImpl.ASimpleDomain
{
    public class APackage : IPackage
    {
        public void RegisterServices(Container container)
        {
            container.RegisterSingleton<ISingletonService, ASingletonService>();
            container.Register<SomeState>();
        }
    }

    public class InMemoryPersistencePackage : IPackage
    {
        public void RegisterServices(Container container)
        {
            // singleton id generator
            container.RegisterInstance(new InMemoryEntityIdGenerator());
 
            // InMemory Repositories
            container.Register(typeof(IRepository<>), typeof(InMemoryRepository<>));
            
            // IQueryable is supported, but should be use with caution, since it bypasses authorization
            container.Register(typeof(IQueryable<>), typeof(InMemoryQueryable<>));
            
            // InMemory Stores
            container.Register(typeof(InMemoryStore<>), typeof(InMemoryStore<>));
            container.RegisterInstance<InMemoryStores>(new InMemoryStores());
        }
    }
}
