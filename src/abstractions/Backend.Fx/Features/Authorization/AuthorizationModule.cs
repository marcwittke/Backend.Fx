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
        private readonly ILogger _logger = Log.Create<AuthorizationModule>();
        private readonly Assembly[] _assemblies;
        
        public AuthorizationModule(Assembly[] assemblies)
        {
            _assemblies = assemblies;
        }

        public void Register(ICompositionRoot compositionRoot)
        {
            RegisterAuthorizingDecorators(compositionRoot);
            RegisterAuthorizationPolicies(compositionRoot);
        }

        private void RegisterAuthorizingDecorators(ICompositionRoot compositionRoot)
        {
            _logger.LogDebug("Registering authorization decorators");
            compositionRoot.RegisterDecorator(ServiceDescriptor.Scoped(typeof(IQueryable<>),
                typeof(AuthorizingQueryable<>)));
            compositionRoot.RegisterDecorator(ServiceDescriptor.Scoped(typeof(IRepository<,>),
                typeof(AuthorizingRepository<,>)));
        }

        private void RegisterAuthorizationPolicies(ICompositionRoot compositionRoot)
        {
            // ReSharper disable once CoVariantArrayConversion
            _logger.LogDebug("Registering authorization services from {Assemblies}", _assemblies);

            var aggregateRootTypes = _assemblies
                .SelectMany(ass => ass.GetTypes())
                .Where(t => t.IsImplementationOfOpenGenericInterface(typeof(IAggregateRoot<>)))
                .ToArray();

            foreach (Type aggregateRootType in aggregateRootTypes)
            {
                Type authorizationPolicyInterfaceType =
                    typeof(IAuthorizationPolicy<>).MakeGenericType(aggregateRootType);
                var authorizationPolicyTypes = _assemblies
                    .GetImplementingTypes(authorizationPolicyInterfaceType)
                    .ToArray();

                if (authorizationPolicyTypes.Length == 0)
                {
                    _logger.LogWarning(
                        "No authorization policies for {AggregateRootType} found", aggregateRootType);
                    return;
                }

                if (authorizationPolicyTypes.Length > 1)
                {
                    throw new InvalidOperationException(
                        $"Multiple authorization policies found for {aggregateRootType.Name}: " +
                        $"[{string.Join(", ", authorizationPolicyTypes.Select(t => t.Name))}]");
                }

                _logger.LogInformation(
                    "Registering scoped authorization service {ServiceType} with implementation {ImplementationType}",
                    authorizationPolicyInterfaceType,
                    authorizationPolicyTypes[0]);
                compositionRoot.Register(ServiceDescriptor.Scoped(authorizationPolicyInterfaceType, authorizationPolicyTypes[0]));
            }
        }
    }
}