namespace Backend.Fx.Testing.InMemoryPersistence
{
    using System.Reflection;

    public abstract class InMemoryPersistenceWithProdDataFixture : InMemoryPersistenceFixture
    {
        protected InMemoryPersistenceWithProdDataFixture(Assembly domainAssembly) : base((bool) false, domainAssembly)
        { }
    }
}