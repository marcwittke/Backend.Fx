using System;
using System.Linq;
using System.Reflection;
using Backend.Fx.DependencyInjection;
using Backend.Fx.Domain;
using Backend.Fx.Features.Persistence;
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
            compositionRoot.RegisterDecorator(ServiceDescriptor.Scoped(typeof(IQueryable<>),
                typeof(AuthorizingQueryable<>)));
            compositionRoot.RegisterDecorator(ServiceDescriptor.Scoped(typeof(IRepository<,>),
                typeof(AuthorizingRepository<,>)));
        }

        private void RegisterAuthorizationPolicies(ICompositionRoot compositionRoot)
        {
            Logger.LogDebug("Registering authorization services from {Assemblies}", _assemblies);

            var aggregateRootTypes = _assemblies
                .SelectMany(ass => ass.GetTypes())
                .Where(t => t.IsImplementationOfOpenGenericInterface(typeof(IAggregateRoot<>)))
                .ToArray();

            foreach (var aggregateRootType in aggregateRootTypes)
            {
                var authorizationPolicyInterfaceType =
                    typeof(IAuthorizationPolicy<>).MakeGenericType(aggregateRootType);
                var authorizationPolicyTypes = _assemblies
                    .GetImplementingTypes(authorizationPolicyInterfaceType)
                    .ToArray();

                if (authorizationPolicyTypes.Length == 0)
                {
                    Logger.LogWarning(
                        "No authorization policies for {AggregateRootType} found", aggregateRootType);
                    return;
                }

                if (authorizationPolicyTypes.Length > 1)
                {
                    throw new InvalidOperationException(
                        $"Multiple authorization policies found for {aggregateRootType.Name}: " +
                        $"[{string.Join(", ", authorizationPolicyTypes.Select(t => t.Name))}]");
                }

                Logger.LogInformation(
                    "Registering scoped authorization service {ServiceType} with implementation {ImplementationType}",
                    authorizationPolicyInterfaceType,
                    authorizationPolicyTypes[0]);
                compositionRoot.Register(ServiceDescriptor.Scoped(authorizationPolicyInterfaceType, authorizationPolicyTypes[0]));
            }
        }
    }
}