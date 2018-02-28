namespace Backend.Fx.Bootstrapping.Modules
{
    using System.Linq;
    using System.Reflection;
    using System.Security.Principal;
    using BuildingBlocks;
    using Environment.Authentication;
    using Environment.MultiTenancy;
    using Patterns.Authorization;
    using Patterns.DataGeneration;
    using Patterns.DependencyInjection;
    using Patterns.EventAggregation;
    using Patterns.Jobs;
    using SimpleInjector;

    /// <summary>
    /// Wires all injected domain services: Current <see cref="IIdentity"/> and current <see cref="TenantId"/> as set while 
    /// beginning the scope. All <see cref="IDomainService"/>s, <see cref="IApplicationService"/>s, <see cref="IAggregateAuthorization{TAggregateRoot}"/>s 
    /// the collections of <see cref="IDomainEventHandler{TDomainEvent}"/>s, <see cref="IJob"/>s and <see cref="InitialDataGenerator"/>s 
    /// found in the given list of domain assemblies.
    /// </summary>
    public abstract class ApplicationModule : SimpleInjectorModule
    {
        private readonly Assembly[] assemblies;
        private IEventAggregator eventAggregator;

        protected ApplicationModule(params Assembly[] domainAssemblies)
        {
            assemblies = domainAssemblies.Concat(new[] {
                typeof(Entity).GetTypeInfo().Assembly,
            }).ToArray();
        }

        public override void Register(ICompositionRoot compositionRoot)
        {
            eventAggregator = new EventAggregator(compositionRoot);        
            base.Register(compositionRoot);
        }

        protected override void Register(Container container, ScopedLifestyle scopedLifestyle)
        {

            // the current IIdentity is resolved using the scoped CurrentIdentityHolder that is maintained when opening a scope
            container.Register<ICurrentTHolder<IIdentity>, CurrentIdentityHolder>();
            container.Register(() => container.GetInstance<ICurrentTHolder<IIdentity>>().Current);

            // same for the current TenantId
            container.Register<ICurrentTHolder<TenantId>, CurrentTenantIdHolder>();
            container.Register(() => container.GetInstance<ICurrentTHolder<TenantId>>().Current);

            RegisterDomainAndApplicationServices(container);

            RegisterAuthorization(container);

            // domain event subsystem
            container.RegisterCollection(typeof(IDomainEventHandler<>), assemblies);
            container.RegisterSingleton<IEventAggregator>(eventAggregator);

            // initial data generation subsystem
            container.RegisterCollection<InitialDataGenerator>(assemblies);

            // all jobs are dynamically registered
            foreach (var scheduledJobType in container.GetTypesToRegister(typeof(IJob), assemblies))
            {
                container.Register(scheduledJobType);
            }
        }

        /// <summary>
        ///     Auto registering all implementors of <see cref="IApplicationService" /> and <see cref="IDomainService" /> with
        ///     their implementations as scoped instances
        /// </summary>
        private void RegisterDomainAndApplicationServices(Container container)
        {
            var serviceRegistrations = container
                    .GetTypesToRegister(typeof(IDomainService), assemblies)
                    .Concat(container.GetTypesToRegister(typeof(IApplicationService), assemblies))
                    .SelectMany(type =>
                                    type.GetTypeInfo()
                                        .ImplementedInterfaces
                                        .Where(i => typeof(IDomainService) != i && typeof(IApplicationService) != i && (i.Namespace.StartsWith("Backend") || assemblies.Contains(i.GetTypeInfo().Assembly)))
                                        .Select(service => new
                                        {
                                            Service = service,
                                            Implementation = type
                                        })
                    );
            foreach (var reg in serviceRegistrations)
            {
                container.Register(reg.Service, reg.Implementation);
            }
        }

        /// <summary>
        ///     Auto registering all aggregate authorization classes
        /// </summary>
        private void RegisterAuthorization(Container container)
        {
            var aggregateRootAuthorizationTypes = container.GetTypesToRegister(typeof(IAggregateAuthorization<>), assemblies).ToArray();
            foreach (var aggregateRootAuthorizationType in aggregateRootAuthorizationTypes)
            {
                var serviceTypes = aggregateRootAuthorizationType
                        .GetTypeInfo()
                        .ImplementedInterfaces
                        .Where(impif => impif.GetTypeInfo().IsGenericType
                                        && impif.GenericTypeArguments.Length == 1
                                        && typeof(AggregateRoot).GetTypeInfo().IsAssignableFrom(impif.GenericTypeArguments[0].GetTypeInfo()));

                foreach (var serviceType in serviceTypes)
                {
                    container.Register(serviceType, aggregateRootAuthorizationType);
                }
            }
        }
    }
}
