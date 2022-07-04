using System.Linq;
using System.Reflection;
using System.Security.Principal;
using Backend.Fx.BuildingBlocks;
using Backend.Fx.Environment.Authentication;
using Backend.Fx.Environment.DateAndTime;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Extensions;
using Backend.Fx.Features.Authorization;
using Backend.Fx.Features.DomainEvents;
using Backend.Fx.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Backend.Fx.Patterns.DependencyInjection
{
    /// <summary>
    /// Wires all public services to be injected as scoped instances provided by the array of domain assemblies:
    /// - <see cref="IApplicationService"/>s
    /// - <see cref="IDomainEventHandler{TDomainEvent}"/>s
    /// - <see cref="IDomainService"/>s
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

            RegisterPermissiveAuthorization(compositionRoot);
        }

        private static void RegisterDomainInfrastructureServices(ICompositionRoot compositionRoot)
        {
            compositionRoot.Register(ServiceDescriptor.Scoped<IClock,WallClock>());
            compositionRoot.Register(ServiceDescriptor.Scoped<IOperation,Operation>());
            compositionRoot.Register(ServiceDescriptor.Scoped<ICurrentTHolder<Correlation>,CurrentCorrelationHolder>());
            compositionRoot.Register(ServiceDescriptor.Scoped<ICurrentTHolder<IIdentity>,CurrentIdentityHolder>());
            compositionRoot.Register(ServiceDescriptor.Scoped<ICurrentTHolder<TenantId>,CurrentTenantIdHolder>());
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
                container.Register(serviceDescriptor);
            }
        }

        /// <summary>
        ///     Auto registering an "allow all" authorization for each aggregate root type
        /// </summary>
        private void RegisterPermissiveAuthorization(ICompositionRoot compositionRoot)
        {
            var aggregateRootTypes = _assemblies.GetImplementingTypes(typeof(AggregateRoot)).ToArray();
            foreach (var aggregateRootType in aggregateRootTypes)
            {
                compositionRoot.Register(
                    new ServiceDescriptor(
                        typeof(IAggregateAuthorization<>).MakeGenericType(aggregateRootType),
                        typeof(AllowAll<>).MakeGenericType(aggregateRootType),
                        ServiceLifetime.Singleton));
            }
        }
    }
}