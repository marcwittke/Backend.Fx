namespace Backend.Fx.Testing.InMemoryPersistence
{
    using Bootstrapping.Modules;
    using Patterns.IdGeneration;
    using SimpleInjector;

    public class InMemoryIdGeneratorsModule : SimpleInjectorModule
    {
        public IEntityIdGenerator EntityIdGenerator { get; } = new InMemoryEntityIdGenerator();

        protected override void Register(Container container, ScopedLifestyle scopedLifestyle)
        {
            container.RegisterInstance(EntityIdGenerator);
        }
    }
}
