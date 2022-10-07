using Backend.Fx.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Features.IdGeneration
{
    public class IdGenerationModule<TId, TIdGenerator> : IModule 
        where TId : struct
        where TIdGenerator : class, IEntityIdGenerator<TId> 
    {
        private readonly TIdGenerator _entityIdGenerator;

        public IdGenerationModule(TIdGenerator entityIdGenerator)
        {
            _entityIdGenerator = entityIdGenerator;
        }

        public void Register(ICompositionRoot compositionRoot)
        {
            compositionRoot.Register(ServiceDescriptor.Singleton(_entityIdGenerator));
        }
    }
}