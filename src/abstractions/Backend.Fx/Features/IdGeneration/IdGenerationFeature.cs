using JetBrains.Annotations;

namespace Backend.Fx.Features.IdGeneration
{
    /// <summary>
    /// use this feature type, when you want your non generic type of <see cref="IEntityIdGenerator{TId}"/> to be injected
    /// </summary>
    /// <typeparam name="TId"></typeparam>
    [PublicAPI]
    public class IdGenerationFeature<TId, TIdGenerator> : Feature 
        where TIdGenerator : class, IEntityIdGenerator<TId> 
    {
        private readonly TIdGenerator _entityIdGenerator;

        public IdGenerationFeature(TIdGenerator entityIdGenerator)
        {
            _entityIdGenerator = entityIdGenerator;
        }
        
        public override void Enable(IBackendFxApplication application)
        {
            application.CompositionRoot.RegisterModules(new IdGenerationModule<TId, TIdGenerator>(_entityIdGenerator));
        }
    }
    
    /// <summary>
    /// use this feature type, when you want the generic <see cref="IEntityIdGenerator{TId}"/> to be injected
    /// </summary>
    /// <typeparam name="TId"></typeparam>
    [PublicAPI]
    public class IdGenerationFeature<TId> : Feature 
    {
        private readonly IEntityIdGenerator<TId> _entityIdGenerator;

        public IdGenerationFeature(IEntityIdGenerator<TId> entityIdGenerator)
        {
            _entityIdGenerator = entityIdGenerator;
        }
        
        public override void Enable(IBackendFxApplication application)
        {
            application.CompositionRoot.RegisterModules(new IdGenerationModule<TId, IEntityIdGenerator<TId>>(_entityIdGenerator));
        }
    }
}