using System.Linq;
using System.Reflection;
using Backend.Fx.DependencyInjection;
using Backend.Fx.Logging;
using Backend.Fx.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.Features.DomainServices
{
    internal class DomainServicesModule : IModule
    {
        private static readonly ILogger Logger = Log.Create<DomainServicesModule>();
        private readonly Assembly[] _assemblies;
        private readonly string _assembliesForLogging;

        public DomainServicesModule(params Assembly[] assemblies)
        {
            _assemblies = assemblies;
            _assembliesForLogging = string.Join(",", _assemblies.Select(ass => ass.GetName().Name));
        }

        public void Register(ICompositionRoot compositionRoot)
        {
            RegisterDomainAndApplicationServices(compositionRoot);
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
                                    && _assemblies.Contains(i.GetTypeInfo().Assembly))
                        .Select(service => new ServiceDescriptor(service, type, ServiceLifetime.Scoped)));


            foreach (var serviceDescriptor in serviceDescriptors)
            {
                Logger.LogDebug("Registering scoped service {ServiceType} with implementation {ImplementationType}",
                    serviceDescriptor.ServiceType.Name,
                    serviceDescriptor.ImplementationType.Name);
                container.Register(serviceDescriptor);
            }
        }
    }
}