using System;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using Backend.Fx.BuildingBlocks;
using Backend.Fx.Environment.Authentication;
using Backend.Fx.Environment.DateAndTime;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Extensions;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.Authorization;
using Backend.Fx.Patterns.EventAggregation.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Backend.Fx.Patterns.DependencyInjection
{
    /// <summary>
    /// Wires all public domain services to be injected as scoped instances provided by the array of domain assemblies:
    /// - <see cref="IApplicationService"/>s
    /// - <see cref="IDomainEventHandler{TDomainEvent}"/>s
    /// - <see cref="IDomainService"/>s
    /// - <see cref="IAggregateAuthorization{TAggregateRoot}"/>s
    /// </summary>
    public class DomainModule : IModule
    {
        private static readonly ILogger Logger = Log.Create<DomainModule>();
        private readonly Assembly[] _assemblies;
        private readonly string _assembliesForLogging;

        public DomainModule(params Assembly[] assemblies)
        {
            _assemblies = assemblies;
            _assembliesForLogging = string.Join(",", _assemblies.Select(ass => ass.GetName().Name));
        }

        public void Register(ICompositionRoot compositionRoot)
        {
            RegisterDomainInfrastructureServices(compositionRoot);

            RegisterDomainAndApplicationServices(compositionRoot);

            RegisterAuthorization(compositionRoot);

            RegisterDomainEventHandlers(compositionRoot);
        }

        private static void RegisterDomainInfrastructureServices(ICompositionRoot compositionRoot)
        {
            compositionRoot.RegisterServiceDescriptor(
                new ServiceDescriptor(typeof(IClock), _ => new WallClock(), ServiceLifetime.Scoped));
            compositionRoot.RegisterServiceDescriptor(
                new ServiceDescriptor(typeof(IOperation), _ => new Operation(), ServiceLifetime.Scoped));
            compositionRoot.RegisterServiceDescriptor(
                new ServiceDescriptor(typeof(ICurrentTHolder<Correlation>), _ => new CurrentCorrelationHolder(),
                    ServiceLifetime.Scoped));
            compositionRoot.RegisterServiceDescriptor(
                new ServiceDescriptor(typeof(ICurrentTHolder<IIdentity>), _ => new CurrentIdentityHolder(),
                    ServiceLifetime.Scoped));
            compositionRoot.RegisterServiceDescriptor(
                new ServiceDescriptor(typeof(ICurrentTHolder<TenantId>), _ => new CurrentTenantIdHolder(),
                    ServiceLifetime.Scoped));
            compositionRoot.RegisterServiceDescriptor(
                new ServiceDescriptor(typeof(IDomainEventAggregator), sp => new DomainEventAggregator(sp),
                    ServiceLifetime.Scoped));
        }

        private void RegisterDomainAndApplicationServices(ICompositionRoot container)
        {
            Logger.LogDebug("Registering domain and application services from {Assemblies}", _assembliesForLogging);

            var serviceDescriptors = _assemblies.GetImplementingTypes(typeof(IDomainService))
                .Concat(_assemblies.GetImplementingTypes(typeof(IApplicationService)))
                .SelectMany(type =>
                    type.GetTypeInfo()
                        .ImplementedInterfaces
                        .Where(i => typeof(IDomainService) != i
                                    && typeof(IApplicationService) != i
                                    && (i.Namespace != null && i.Namespace.StartsWith("Backend")
                                        || _assemblies.Contains(i.GetTypeInfo().Assembly)))
                        .Select(service => new ServiceDescriptor(service, type, ServiceLifetime.Scoped)));


            foreach (var serviceDescriptor in serviceDescriptors)
            {
                Logger.LogDebug("Registering scoped service {ServiceType} with implementation {ImplementationType}",
                    serviceDescriptor.ServiceType.Name,
                    serviceDescriptor.ImplementationType.Name);
                container.RegisterServiceDescriptor(serviceDescriptor);
            }
        }

        /// <summary>
        ///     Auto registering all aggregate authorization classes
        /// </summary>
        private void RegisterAuthorization(ICompositionRoot compositionRoot)
        {
            Logger.LogDebug("Registering authorization services from {Assemblies}", _assembliesForLogging);
            var aggregateRootAuthorizationTypes =
                _assemblies.GetImplementingTypes(typeof(IAggregateAuthorization<>)).ToArray();

            foreach (Type aggregateAuthorizationType in aggregateRootAuthorizationTypes)
            {
                var serviceTypes = aggregateAuthorizationType
                    .GetTypeInfo()
                    .ImplementedInterfaces
                    .Where(i => i.GetTypeInfo().IsGenericType
                                && i.GenericTypeArguments.Length == 1
                                && typeof(AggregateRoot).GetTypeInfo()
                                    .IsAssignableFrom(i.GenericTypeArguments[0].GetTypeInfo()));

                foreach (Type serviceType in serviceTypes)
                {
                    Logger.LogDebug(
                        "Registering scoped authorization service {ServiceType} with implementation {ImplementationType}",
                        serviceType.Name,
                        aggregateAuthorizationType.Name);
                    compositionRoot.RegisterServiceDescriptor(new ServiceDescriptor(serviceType,
                        aggregateAuthorizationType, ServiceLifetime.Scoped));
                }
            }
        }

        private void RegisterDomainEventHandlers(ICompositionRoot compositionRoot)
        {
            foreach (Type domainEventType in _assemblies.GetImplementingTypes(typeof(IDomainEvent)))
            {
                Type handlerTypeForThisDomainEventType = typeof(IDomainEventHandler<>).MakeGenericType(domainEventType);

                var serviceDescriptors = _assemblies
                    .GetImplementingTypes(handlerTypeForThisDomainEventType)
                    .Select(t => new ServiceDescriptor(handlerTypeForThisDomainEventType, t, ServiceLifetime.Scoped))
                    .ToArray();
                
                compositionRoot.RegisterServiceDescriptors(serviceDescriptors);
            }
        }
    }
}