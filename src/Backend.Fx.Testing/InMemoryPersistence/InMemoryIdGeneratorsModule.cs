namespace Backend.Fx.Testing.InMemoryPersistence
{
    using Bootstrapping;
    using Bootstrapping.Modules;
    using Patterns.IdGeneration;
    using SimpleInjector;

    public class InMemoryIdGeneratorsModule : SimpleInjectorModule
    {
        public IEntityIdGenerator EntityIdGenerator { get; } = new InMemoryEntityIdGenerator();

        public InMemoryIdGeneratorsModule(SimpleInjectorCompositionRoot compositionRoot) : base(compositionRoot)
        {}

        protected override void Register(Container container, ScopedLifestyle scopedLifestyle)
        {
            container.RegisterSingleton(EntityIdGenerator);
        }
    }
}
