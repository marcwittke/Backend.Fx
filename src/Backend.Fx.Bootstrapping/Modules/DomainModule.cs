namespace Backend.Fx.Bootstrapping.Modules
{
    using System.Linq;
    using System.Reflection;
    using System.Security.Principal;
    using BuildingBlocks;
    using Environment.Authentication;
    using Environment.MultiTenancy;
    using Logging;
    using Patterns.Authorization;
    using Patterns.DataGeneration;
    using Patterns.DependencyInjection;
    using Patterns.EventAggregation.Domain;
    using Patterns.Jobs;
    using SimpleInjector;

    /// <summary>
    /// Wires all injected domain services: Current <see cref="IIdentity"/> and current <see cref="TenantId"/> as set while 
    /// beginning the scope. All <see cref="IDomainService"/>s, <see cref="IApplicationService"/>s, <see cref="IAggregateAuthorization{TAggregateRoot}"/>s 
    /// the collections of <see cref="IDomainEventHandler{TDomainEvent}"/>s, <see cref="IJob"/>s and <see cref="InitialDataGenerator"/>s 
    /// found in the given list of domain assemblies.
    /// </summary>
    public abstract class DomainModule : SimpleInjectorModule
    {
        private static readonly ILogger Logger = LogManager.Create<DomainModule>();
        private readonly Assembly[] assemblies;
        private IDomainEventAggregator domainEventAggregator;

        protected DomainModule(params Assembly[] domainAssemblies)
        {
            assemblies = domainAssemblies.Concat(new[] {
                typeof(Entity).GetTypeInfo().Assembly,
            }).ToArray();
        }

        public override void Register(ICompositionRoot compositionRoot)
        {
            domainEventAggregator = new DomainEventAggregator(compositionRoot);        
            base.Register(compositionRoot);
        }

        protected override void Register(Container container, ScopedLifestyle scopedLifestyle)
        {
            // the current IIdentity is resolved using the scoped CurrentIdentityHolder that is maintained when opening a scope
            Logger.Debug($"Registering {nameof(CurrentIdentityHolder)} as {nameof(ICurrentTHolder<IIdentity>)}");
            container.Register<ICurrentTHolder<IIdentity>, CurrentIdentityHolder>();

            // same for the current TenantId
            Logger.Debug($"Registering {nameof(CurrentTenantIdHolder)} as {nameof(ICurrentTHolder<TenantId>)}");
            container.Register<ICurrentTHolder<TenantId>, CurrentTenantIdHolder>();
            
            container.RegisterDomainAndApplicationServices(assemblies);

            RegisterAuthorization(container);

            // domain event subsystem
            Logger.Debug($"Registering domain event handlers from {string.Join(",", assemblies.Select(ass => ass.GetName().Name))}");
            container.RegisterCollection(typeof(IDomainEventHandler<>), assemblies);
            Logger.Debug("Registering singleton event aggregator instance");
            container.RegisterSingleton(domainEventAggregator);

            // initial data generation subsystem
            Logger.Debug($"Registering initial data generators from {string.Join(",", assemblies.Select(ass => ass.GetName().Name))}");
            container.RegisterCollection<InitialDataGenerator>(assemblies);

            // all jobs are dynamically registered
            foreach (var scheduledJobType in container.GetTypesToRegister(typeof(IJob), assemblies))
            {
                Logger.Debug($"Registering {scheduledJobType.Name}");
                container.Register(scheduledJobType);
            }
        }

        /// <summary>
        ///     Auto registering all aggregate authorization classes
        /// </summary>
        private void RegisterAuthorization(Container container)
        {
            Logger.Debug($"Registering authorization services from {string.Join(",", assemblies.Select(ass => ass.GetName().Name))}");
            var aggregateRootAuthorizationTypes = container.GetTypesToRegister(typeof(IAggregateAuthorization<>), assemblies).ToArray();
            foreach (var aggregateAuthorizationType in aggregateRootAuthorizationTypes)
            {
                var serviceTypes = aggregateAuthorizationType
                        .GetTypeInfo()
                        .ImplementedInterfaces
                        .Where(impif => impif.GetTypeInfo().IsGenericType
                                        && impif.GenericTypeArguments.Length == 1
                                        && typeof(AggregateRoot).GetTypeInfo().IsAssignableFrom(impif.GenericTypeArguments[0].GetTypeInfo()));

                foreach (var serviceType in serviceTypes)
                {
                    Logger.Debug($"Registering scoped authorization service {serviceType.Name} with implementation {aggregateAuthorizationType.Name}");
                    container.Register(serviceType, aggregateAuthorizationType);
                }
            }
        }
    }
}
