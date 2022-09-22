using System;
using System.Linq;
using System.Reflection;
using Backend.Fx.DependencyInjection;
using Backend.Fx.Domain;
using Backend.Fx.Logging;
using Backend.Fx.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.Features.Authorization
{
    internal class AuthorizationModule : IModule
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
            RegisterAuthorizingDecorators(compositionRoot);
            RegisterAuthorizationPolicies(compositionRoot);
        }

        private static void RegisterAuthorizingDecorators(ICompositionRoot compositionRoot)
        {
            Logger.LogDebug("Registering authorization decorators");
            compositionRoot.RegisterDecorator(ServiceDescriptor.Scoped(typeof(IAsyncAggregateQueryable<>), typeof(AuthorizingQueryable<>)));
            compositionRoot.RegisterDecorator(ServiceDescriptor.Scoped(typeof(IRepository<>), typeof(AuthorizingRepository<>)));
        }
        
        private void RegisterAuthorizationPolicies(ICompositionRoot compositionRoot)
        {
            Logger.LogDebug("Registering authorization services from {Assemblies}", _assembliesForLogging);

            var aggregateRootTypes = _assemblies.GetImplementingTypes(typeof(AggregateRoot)).ToArray();
            foreach (var aggregateRootType in aggregateRootTypes)
            {
                var aggregateAuthorizationTypes = _assemblies
                    .GetImplementingTypes(typeof(IAuthorizationPolicy<>).MakeGenericType(aggregateRootType))
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