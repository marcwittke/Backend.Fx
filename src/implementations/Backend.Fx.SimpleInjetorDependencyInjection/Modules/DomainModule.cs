using System;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using Backend.Fx.BuildingBlocks;
using Backend.Fx.Environment.Authentication;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.Authorization;
using Backend.Fx.Patterns.DataGeneration;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Domain;
using Backend.Fx.Patterns.Jobs;
using SimpleInjector;

namespace Backend.Fx.SimpleInjectorDependencyInjection.Modules
{
    /// <summary>
    /// Wires all injected domain services: Current <see cref="IIdentity"/> and current <see cref="TenantId"/> as set while 
    /// beginning the scope. All <see cref="IDomainService"/>s, <see cref="IApplicationService"/>s, <see cref="IAggregateAuthorization{TAggregateRoot}"/>s 
    /// the collections of <see cref="IDomainEventHandler{TDomainEvent}"/>s, <see cref="IJob"/>s and <see cref="DataGenerator"/>s 
    /// found in the given list of domain assemblies.
    /// </summary>
    public abstract class DomainModule : SimpleInjectorModule
    {
        private readonly IExceptionLogger _exceptionLogger;
        private static readonly ILogger Logger = LogManager.Create<DomainModule>();
        private readonly Assembly[] _assemblies;
        private Func<DomainEventAggregator> _domainEventAggregatorFactory;
        private readonly string _assembliesForLogging;

        protected DomainModule(IExceptionLogger exceptionLogger, params Assembly[] domainAssemblies)
        {
            _exceptionLogger = exceptionLogger;
            _assemblies = domainAssemblies.Concat(new[] {
                typeof(Entity).GetTypeInfo().Assembly,
            }).ToArray();

            _assembliesForLogging = string.Join(",", _assemblies.Select(ass => ass.GetName().Name));
        }

        public override void Register(ICompositionRoot compositionRoot)
        {
            _domainEventAggregatorFactory = ()=>new DomainEventAggregator(compositionRoot);        
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
            
            container.RegisterDomainAndApplicationServices(_assemblies);

            RegisterAuthorization(container);

            // domain event subsystem
            Logger.Debug($"Registering domain event handlers from {_assembliesForLogging}");
            container.Collection.Register(typeof(IDomainEventHandler<>), _assemblies);
            Logger.Debug("Registering event aggregator");
            container.Register<IDomainEventAggregator>(_domainEventAggregatorFactory);

            // initial data generation subsystem
            Logger.Debug($"Registering initial data generators from {_assembliesForLogging}");
            container.Collection.Register<DataGenerator>(_assemblies);

            // all jobs are dynamically registered
            foreach (var jobType in container.GetTypesToRegister(typeof(IJob), _assemblies))
            {
                Logger.Debug($"Registering {jobType.Name}");
                container.Register(jobType);
            }

            container.Register(typeof(IJobExecutor<>), typeof(JobExecutor<>));
            container.RegisterDecorator(typeof(IJobExecutor<>), typeof(UnitOfWorkJobExecutor<>));
            container.RegisterDecorator(typeof(IJobExecutor<>), typeof(ExceptionLoggingJobExecutor<>));
        }


        /// <summary>
        ///     Auto registering all aggregate authorization classes
        /// </summary>
        private void RegisterAuthorization(Container container)
        {
            Logger.Debug($"Registering authorization services from {_assembliesForLogging}");
            var aggregateRootAuthorizationTypes = container.GetTypesToRegister(typeof(IAggregateAuthorization<>), _assemblies).ToArray();
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
