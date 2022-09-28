using Backend.Fx.DependencyInjection;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Features.IdGeneration
{
    [PublicAPI]
    public class IdGenerationFeature : Feature
    {
        private readonly IEntityIdGenerator _entityIdGenerator;

        public IdGenerationFeature(IEntityIdGenerator entityIdGenerator)
        {
            _entityIdGenerator = entityIdGenerator;
        }
        
        public override void Enable(IBackendFxApplication application)
        {
            application.CompositionRoot.RegisterModules(new IdGenerationModule(_entityIdGenerator));
        }
    }

    public class IdGenerationModule : IModule
    {
        private readonly IEntityIdGenerator _entityIdGenerator;

        public IdGenerationModule(IEntityIdGenerator entityIdGenerator)
        {
            _entityIdGenerator = entityIdGenerator;
        }

        public void Register(ICompositionRoot compositionRoot)
        {
            compositionRoot.Register(ServiceDescriptor.Singleton(_entityIdGenerator));
        }
    }
}