using System;
using System.Linq;
using System.Reflection;
using Backend.Fx.BuildingBlocks;
using Backend.Fx.Extensions;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Backend.Fx.Features.Authorization
{
    public class AuthorizingApplication : BackendFxApplicationDecorator
    {
        public AuthorizingApplication(IBackendFxApplication application) : base(application)
        {
            application.CompositionRoot.RegisterModules(new AuthorizationModule(Assemblies));
        }
    }

    public class AuthorizationModule : IModule
    {
        private static readonly ILogger Logger = Log.Create<AuthorizationModule>();
        private readonly Assembly[] _assemblies;
        private readonly string _assembliesForLogging;

        public AuthorizationModule(Assembly[] assemblies)
        {
            _assemblies = assemblies;
            _assembliesForLogging = string.Join(",", _assemblies.Select(ass => ass.GetName().Name));
        }

        public void Register(ICompositionRoot compositionRoot)
        {
            Logger.LogDebug("Registering authorization services from {Assemblies}", _assembliesForLogging);

            var aggregateRootTypes = _assemblies.GetImplementingTypes(typeof(AggregateRoot)).ToArray();
            foreach (var aggregateRootType in aggregateRootTypes)
            {
                var aggregateAuthorizationTypes = _assemblies
                    .GetImplementingTypes(typeof(IAggregateAuthorization<>).MakeGenericType(aggregateRootType))
                    .ToArray();

                foreach (Type aggregateAuthorizationType in aggregateAuthorizationTypes)
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
                        compositionRoot.Register(
                            new ServiceDescriptor(
                                serviceType,
                                aggregateAuthorizationType,
                                ServiceLifetime.Scoped));
                    }
                }
            }
        }
    }
}