using System;
using System.Security.Principal;
using Backend.Fx.Environment.Authentication;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Domain;
using Backend.Fx.Patterns.EventAggregation.Integration;
using SimpleInjector;

namespace Backend.Fx.SimpleInjectorDependencyInjection.Modules
{
    public class InfrastructureModule : SimpleInjectorModule
    {
        private readonly IExceptionLogger _exceptionLogger;
        private readonly IEventBus _eventBus;
        private static readonly ILogger Logger = LogManager.Create<InfrastructureModule>();

        private Func<DomainEventAggregator> _domainEventAggregatorFactory;

        public InfrastructureModule(IExceptionLogger exceptionLogger, IEventBus eventBus)
        {
            _exceptionLogger = exceptionLogger;
            _eventBus = eventBus;
        }

        public override void Register(ICompositionRoot compositionRoot)
        {
            _domainEventAggregatorFactory = () => new DomainEventAggregator(compositionRoot);
            base.Register(compositionRoot);
        }

        protected override void Register(Container container, ScopedLifestyle scopedLifestyle)
        {
            container.RegisterInstance(_exceptionLogger);

            // the current IIdentity is resolved using the scoped CurrentIdentityHolder that is maintained when opening a scope
            Logger.Debug($"Registering {nameof(CurrentIdentityHolder)} as {nameof(ICurrentTHolder<IIdentity>)}");
            container.Register<ICurrentTHolder<IIdentity>, CurrentIdentityHolder>();

            // same for the current TenantId
            Logger.Debug($"Registering {nameof(CurrentTenantIdHolder)} as {nameof(ICurrentTHolder<TenantId>)}");
            container.Register<ICurrentTHolder<TenantId>, CurrentTenantIdHolder>();

            Logger.Debug("Registering event aggregator");
            container.Register<IDomainEventAggregator>(_domainEventAggregatorFactory);

            // integration event subsystem
            Logger.Debug("Registering event bus");
            container.RegisterInstance(_eventBus);

            container.Register<IEventBusScope, EventBusScope>();

        }
    }
}