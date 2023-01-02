using Backend.Fx.ExecutionPipeline;
using JetBrains.Annotations;

namespace Backend.Fx.Features.DomainEvents
{
    /// <summary>
    /// The feature "Domain Events" provides you with a domain event aggregator, that will be injected as a scoped
    /// instance and generic domain event handlers that will also be injected as scoped instances. You can publish
    /// arbitrary domain events using the <see cref="IDomainEventAggregator"/> instance, but domain events won't be
    /// raised until the <see cref="IOperation"/>is completing.
    /// Failures when handling domain events will result in canceling the whole operation, thus in rolling back a
    /// possible transaction.
    /// </summary>
    [PublicAPI]
    public class DomainEventsFeature : Feature
    {
        public override void Enable(IBackendFxApplication application)
        {
            application.CompositionRoot.RegisterModules(new DomainEventsModule(application.Assemblies));
        }
    }
}