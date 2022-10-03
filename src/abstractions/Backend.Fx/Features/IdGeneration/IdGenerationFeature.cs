using Backend.Fx.DependencyInjection;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Features.IdGeneration
{
    [PublicAPI]
    public class IdGenerationFeature<TId> : Feature where TId : struct
    {
        private readonly IEntityIdGenerator<TId> _entityIdGenerator;

        public IdGenerationFeature(IEntityIdGenerator<TId> entityIdGenerator)
        {
            _entityIdGenerator = entityIdGenerator;
        }
        
        public override void Enable(IBackendFxApplication application)
        {
            application.CompositionRoot.RegisterModules(new IdGenerationModule<TId>(_entityIdGenerator));
        }
    }

    public class IdGenerationModule<TId> : IModule where TId : struct
    {
        private readonly IEntityIdGenerator<TId> _entityIdGenerator;

        public IdGenerationModule(IEntityIdGenerator<TId> entityIdGenerator)
        {
            _entityIdGenerator = entityIdGenerator;
        }

        public void Register(ICompositionRoot compositionRoot)
        {
            compositionRoot.Register(ServiceDescriptor.Singleton(_entityIdGenerator));
        }
    }
}