using System.Linq;
using System.Reflection;
using Backend.Fx.BuildingBlocks;
using Backend.Fx.Logging;
using SimpleInjector;

namespace Backend.Fx.SimpleInjectorDependencyInjection
{
    public static class SimpleInjectorContainerEx
    {
        private static readonly ILogger Logger = LogManager.Create(typeof(SimpleInjectorContainerEx));

        /// <summary>
        ///     Auto registering all implementors of <see cref="IApplicationService" /> and <see cref="IDomainService" /> with
        ///     their implementations as scoped instances
        /// </summary>
        public static void RegisterDomainAndApplicationServices(this Container container, Assembly[] assemblies)
        {
            Logger.Debug($"Registering domain and application services from {string.Join(",", assemblies.Select(ass => ass.GetName().Name))}");
            var serviceRegistrations = container
                                       .GetTypesToRegister(typeof(IDomainService), assemblies)
                                       .Concat(container.GetTypesToRegister(typeof(IApplicationService), assemblies))
                                       .SelectMany(type =>
                                                       type.GetTypeInfo()
                                                           .ImplementedInterfaces
                                                           .Where(i => typeof(IDomainService) != i 
                                                                       && typeof(IApplicationService) != i 
                                                                       && (i.Namespace != null && i.Namespace.StartsWith("Backend") 
                                                                           || assemblies.Contains(i.GetTypeInfo().Assembly)))
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
    }
}
