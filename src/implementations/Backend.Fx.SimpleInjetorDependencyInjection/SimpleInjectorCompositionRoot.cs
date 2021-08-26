using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using Backend.Fx.BuildingBlocks;
using Backend.Fx.Environment.Authentication;
using Backend.Fx.Environment.DateAndTime;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.Authorization;
using Backend.Fx.Patterns.DataGeneration;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Domain;
using Backend.Fx.Patterns.EventAggregation.Integration;
using Backend.Fx.Patterns.Jobs;
using SimpleInjector;
using SimpleInjector.Advanced;
using SimpleInjector.Lifestyles;

namespace Backend.Fx.SimpleInjectorDependencyInjection
{
    /// <summary>
    ///     Provides a reusable composition root assuming Simple Injector as container
    /// </summary>
    public class SimpleInjectorCompositionRoot : ICompositionRoot
    {
        private readonly IMessageBus _messageBus;
        private static readonly ILogger Logger = LogManager.Create<SimpleInjectorCompositionRoot>();

        private int _scopeSequenceNumber = 1;
        private readonly Assembly[] _assemblies;
        private readonly string _assembliesForLogging;

        /// <summary>
        /// This constructor creates a composition root that prefers scoped lifestyle
        /// </summary>
        public SimpleInjectorCompositionRoot(IMessageBus messageBus, params Assembly[] assemblies)
            : this(messageBus, assemblies, new ScopedLifestyleBehavior(), new AsyncScopedLifestyle())
        { }

        private SimpleInjectorCompositionRoot(IMessageBus messageBus, Assembly[] assemblies, ILifestyleSelectionBehavior lifestyleBehavior, ScopedLifestyle scopedLifestyle)
        {
            Logger.Info("Initializing SimpleInjector");
            
            _messageBus = messageBus;
            _assemblies = assemblies ?? Array.Empty<Assembly>();
            _assembliesForLogging = string.Join(",", _assemblies.Select(ass => ass.GetName().Name));
            
            Container.Options.LifestyleSelectionBehavior = lifestyleBehavior;
            Container.Options.DefaultScopedLifestyle = scopedLifestyle;
            InstanceProvider = new SimpleInjectorInstanceProvider(Container);
            
            // the basic types that are open for decorators
            Container.Register<IClock, WallClock>();
            Container.Register<IOperation, Operation>();
            
            // holder types that provide access to cross cutting, scope-local state
            Container.Register<ICurrentTHolder<TenantId>, CurrentTenantIdHolder>();
            Container.Register<ICurrentTHolder<IIdentity>, CurrentIdentityHolder>();
            Container.Register<ICurrentTHolder<Correlation>, CurrentCorrelationHolder>();
            
            RegisterDomainAndApplicationServices(Container);

            RegisterAuthorization(Container);

            // all jobs are dynamically registered
            foreach (Type jobType in Container.GetTypesToRegister(typeof(IJob), _assemblies))
            {
                Logger.Debug($"Registering {jobType.Name}");
                Container.Register(jobType);
            }

            // domain event aggregator
            Container.Register<IDomainEventAggregator>(() => new DomainEventAggregator(new SimpleInjectorDomainEventHandlerProvider(Container)));
            
            // domain event handlers
            foreach (Type domainEventHandlerType in Container.GetTypesToRegister(typeof(IDomainEventHandler<>), _assemblies))
            {
                Logger.Debug($"Appending {domainEventHandlerType.Name} to list of IDomainEventHandler");
                Container.Collection.Append(typeof(IDomainEventHandler<>), domainEventHandlerType);
            }
            
            // integration event message bus scope
            Container.Register<IMessageBusScope>(() => new MessageBusScope(
                _messageBus,
                Container.GetInstance<ICurrentTHolder<Correlation>>(),
                Container.GetInstance<ICurrentTHolder<TenantId>>()));
            
            // integration message handlers
            foreach (Type integrationMessageHandlerType in Container.GetTypesToRegister(typeof(IIntegrationMessageHandler<>), _assemblies))
            {
                Logger.Debug($"Registering {integrationMessageHandlerType.Name}");
                Container.Register(integrationMessageHandlerType);
            }
            
            // data generators
            Container.Collection.Register<IDataGenerator>(Container.GetTypesToRegister(typeof(IDataGenerator), _assemblies));
        }

        public Container Container { get; } = new Container();


        #region ICompositionRoot implementation

        public void Verify()
        {
            Logger.Info("container is being verified");
            try
            {
                Container.Verify(VerificationOption.VerifyAndDiagnose);
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "container configuration invalid");
                throw;
            }
        }


        public object GetInstance(Type serviceType)
        {
            return Container.GetInstance(serviceType);
        }

        public IEnumerable GetInstances(Type serviceType)
        {
            return Container.GetAllInstances(serviceType);
        }

        public T GetInstance<T>() where T : class
        {
            return Container.GetInstance<T>();
        }

        public IEnumerable<T> GetInstances<T>() where T : class
        {
            return Container.GetAllInstances<T>();
        }
        
        /// <inheritdoc />
        public IInjectionScope BeginScope()
        {
            return new SimpleInjectorInjectionScope(Interlocked.Increment(ref _scopeSequenceNumber), AsyncScopedLifestyle.BeginScope(Container));
        }

        public IInstanceProvider InstanceProvider { get; }
        #endregion

        #region IDisposable implementation
        public void Dispose()
        {
            Container?.Dispose();
        }
        #endregion

        private void RegisterDomainAndApplicationServices(Container container)
        {
            Logger.Debug($"Registering domain and application services from {string.Join(",", _assemblies.Select(ass => ass.GetName().Name))}");
            var serviceRegistrations = container
                                       .GetTypesToRegister(typeof(IDomainService), _assemblies)
                                       .Concat(container.GetTypesToRegister(typeof(IApplicationService), _assemblies))
                                       .SelectMany(type =>
                                                       type.GetTypeInfo()
                                                           .ImplementedInterfaces
                                                           .Where(i => typeof(IDomainService) != i
                                                                       && typeof(IApplicationService) != i
                                                                       && (i.Namespace != null && i.Namespace.StartsWith("Backend")
                                                                           || _assemblies.Contains(i.GetTypeInfo().Assembly)))
                                                           .Select(service => new
                                                                              {
                                                                                  Service = service,
                                                                                  Implementation = type
                                                                              })
                                       );
            foreach (var reg in serviceRegistrations)
            {
                Logger.Debug($"Registering scoped service {reg.Service.Name} with implementation {reg.Implementation.Name}");
                container.Register(reg.Service, reg.Implementation);
            }
        }

        /// <summary>
        ///     Auto registering all aggregate authorization classes
        /// </summary>
        private void RegisterAuthorization(Container container)
        {
            Logger.Debug($"Registering authorization services from {_assembliesForLogging}");
            var aggregateRootAuthorizationTypes = container.GetTypesToRegister(typeof(IAggregateAuthorization<>), _assemblies).ToArray();
            foreach (Type aggregateAuthorizationType in aggregateRootAuthorizationTypes)
            {
                var serviceTypes = aggregateAuthorizationType
                        .GetTypeInfo()
                        .ImplementedInterfaces
                        .Where(impif => impif.GetTypeInfo().IsGenericType
                                        && impif.GenericTypeArguments.Length == 1
                                        && typeof(AggregateRoot).GetTypeInfo().IsAssignableFrom(impif.GenericTypeArguments[0].GetTypeInfo()));

                foreach (Type serviceType in serviceTypes)
                {
                    Logger.Debug($"Registering scoped authorization service {serviceType.Name} with implementation {aggregateAuthorizationType.Name}");
                    container.Register(serviceType, aggregateAuthorizationType);
                }
            }
        }
        
        private class SimpleInjectorDomainEventHandlerProvider : IDomainEventHandlerProvider
        {
            private readonly Container _container;

            public SimpleInjectorDomainEventHandlerProvider(Container container)
            {
                _container = container;
            }

            public IEnumerable<IDomainEventHandler<TDomainEvent>> GetAllEventHandlers<TDomainEvent>() where TDomainEvent : IDomainEvent
            {
                return _container.GetAllInstances<IDomainEventHandler<TDomainEvent>>();
            }
        }
        
        /// <summary>
        ///     A behavior that defaults to scoped life style for injected instances
        /// </summary>
        private class ScopedLifestyleBehavior : ILifestyleSelectionBehavior
        {
            public Lifestyle SelectLifestyle(Type implementationType)
            {
                return Lifestyle.Scoped;
            }
        }
    }
}