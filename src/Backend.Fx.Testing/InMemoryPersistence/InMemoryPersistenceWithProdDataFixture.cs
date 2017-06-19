namespace Backend.Fx.Testing.InMemoryPersistence
{
    using System;
    using System.Reflection;
    using JetBrains.Annotations;
    using SimpleInjector;

    public abstract class InMemoryPersistenceWithProdDataFixture : InMemoryPersistenceFixture
    {
        protected InMemoryPersistenceWithProdDataFixture(Assembly domainAssembly, [CanBeNull] Action<Container> additionalContainerConfig = null) 
            : base(false, domainAssembly, additionalContainerConfig)
        { }
    }
}