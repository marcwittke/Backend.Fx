namespace Backend.Fx.Testing.InMemoryPersistence
{
    using System.Reflection;

    public abstract class InMemoryPersistenceWithDemoDataFixture : InMemoryPersistenceFixture
    {
        protected InMemoryPersistenceWithDemoDataFixture(Assembly domainAssembly) : base(true, domainAssembly)
        { }
    }
}