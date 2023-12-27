﻿using System;
using System.Linq;
using System.Reflection;
using Backend.Fx.BuildingBlocks;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.Authorization;
using Backend.Fx.Patterns.DataGeneration;
using Backend.Fx.Patterns.EventAggregation.Domain;
using Backend.Fx.Patterns.EventAggregation.Integration;
using Backend.Fx.Patterns.Jobs;
using Microsoft.Extensions.Logging;
using SimpleInjector;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Backend.Fx.SimpleInjectorDependencyInjection.Modules
{
    /// <summary>
    /// Wires all public domain services to be injected as scoped instances provided by the array of domain assemblies:
    /// - <see cref="IDomainService"/>s
    /// - <see cref="IAggregateAuthorization{TAggregateRoot}"/>s
    /// - <see cref="IJob"/>s
    /// - <see cref="DataGenerator"/>s 
    /// - the collections of <see cref="IDomainEventHandler{TDomainEvent}"/>s
    /// - <see cref="IIntegrationMessageHandler{TIntegrationEvent}"/>'s
    /// </summary>
    public class SimpleInjectorDomainModule : SimpleInjectorModule
    {
        private static readonly ILogger Logger = Log.Create<SimpleInjectorDomainModule>();
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
                Logger.LogDebug("Registering {JobType}", jobType.Name);
                container.Register(jobType);
            }

            // domain event handlers
            foreach (Type domainEventHandlerType in container.GetTypesToRegister(typeof(IDomainEventHandler<>), _domainAssemblies))
            {
                Logger.LogDebug("Appending {DomainEventHandlerType} to list of IDomainEventHandler", domainEventHandlerType.Name);
                container.Collection.Append(typeof(IDomainEventHandler<>), domainEventHandlerType);
            }
            
            // integration message handlers
            foreach (Type integrationMessageHandlerType in container.GetTypesToRegister(typeof(IIntegrationMessageHandler<>), _domainAssemblies))
            {
                Logger.LogDebug("Registering {IntegrationEventHandler}", integrationMessageHandlerType.Name);
                container.Register(integrationMessageHandlerType);
            }
        }

        private void RegisterDomainAndApplicationServices(Container container)
        {
            Logger.LogDebug("Registering domain and application services from {DomainAssemblies}",
                string.Join(",", _domainAssemblies.Select(ass => ass.GetName().Name)));
            var serviceRegistrations = container
                                       .GetTypesToRegister(typeof(IDomainService), _domainAssemblies)
                                       .SelectMany(type =>
                                                       type.GetTypeInfo()
                                                           .ImplementedInterfaces
                                                           .Where(i => typeof(IDomainService) != i
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
                Logger.LogDebug("Registering scoped service {ServiceType} with implementation {ImplementationType}",
                    reg.Service.Name,
                    reg.Implementation.Name);
                container.Register(reg.Service, reg.Implementation);
            }
        }

        /// <summary>
        ///     Auto registering all aggregate authorization classes
        /// </summary>
        private void RegisterAuthorization(Container container)
        {
            Logger.LogDebug("Registering authorization services from {DomainAssemblies}", _domainAssembliesForLogging);
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
                    Logger.LogDebug("Registering scoped authorization service {ServiceType} with implementation {ImplementationType}",
                        serviceType.Name,
                        aggregateAuthorizationType.Name);
                    container.Register(serviceType, aggregateAuthorizationType);
                }
            }
        }
    }
}
