namespace Backend.Fx.Testing.InMemoryPersistence
{
    using System;
    using System.Reflection;
    using JetBrains.Annotations;
    using SimpleInjector;

    public abstract class InMemoryPersistenceWithDemoDataFixture : InMemoryPersistenceFixture
    {
        protected InMemoryPersistenceWithDemoDataFixture(Assembly domainAssembly, [CanBeNull] Action<Container> additionalContainerConfig) 
            : base(true, domainAssembly, additionalContainerConfig)
        { }
    }
}