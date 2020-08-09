using System;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using Backend.Fx.BuildingBlocks;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.Authorization;
using Backend.Fx.Patterns.DataGeneration;
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
    public class SimpleInjectorDomainModule : SimpleInjectorModule
    {
        private static readonly ILogger Logger = LogManager.Create<SimpleInjectorDomainModule>();
        private readonly Assembly[] _domainAssemblies;
        private readonly string _domainAssembliesForLogging;

        public SimpleInjectorDomainModule(params Assembly[] domainAssemblies)
        {
            _domainAssemblies = domainAssemblies;
            _domainAssembliesForLogging = string.Join(",", _domainAssemblies.Select(ass => ass.GetName().Name));
        }

        protected override void Register(Container container, ScopedLifestyle scopedLifestyle)
        {
            RegisterDomainAndApplicationServices(container);

            RegisterAuthorization(container);

            // all jobs are dynamically registered
            foreach (Type jobType in container.GetTypesToRegister(typeof(IJob), _domainAssemblies))
            {
                Logger.Debug($"Registering {jobType.Name}");
                container.Register(jobType);
            }

            // domain event handlers
            foreach (Type domainEventHandlerType in container.GetTypesToRegister(typeof(IDomainEventHandler<>), _domainAssemblies))
            {
                Logger.Debug($"Appending {domainEventHandlerType.Name} to list of IDomainEventHandler");
                container.Collection.Append(typeof(IDomainEventHandler<>), domainEventHandlerType);
            }
        }

        private void RegisterDomainAndApplicationServices(Container container)
        {
            Logger.Debug($"Registering domain and application services from {string.Join(",", _domainAssemblies.Select(ass => ass.GetName().Name))}");
            var serviceRegistrations = container
                                       .GetTypesToRegister(typeof(IDomainService), _domainAssemblies)
                                       .Concat(container.GetTypesToRegister(typeof(IApplicationService), _domainAssemblies))
                                       .SelectMany(type =>
                                                       type.GetTypeInfo()
                                                           .ImplementedInterfaces
                                                           .Where(i => typeof(IDomainService) != i
                                                                       && typeof(IApplicationService) != i
                                                                       && (i.Namespace != null && i.Namespace.StartsWith("Backend")
                                                                           || _domainAssemblies.Contains(i.GetTypeInfo().Assembly)))
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
            Logger.Debug($"Registering authorization services from {_domainAssembliesForLogging}");
            var aggregateRootAuthorizationTypes = container.GetTypesToRegister(typeof(IAggregateAuthorization<>), _domainAssemblies).ToArray();
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
    }
}
