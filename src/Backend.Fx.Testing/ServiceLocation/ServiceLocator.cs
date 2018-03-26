namespace Backend.Fx.Testing.ServiceLocation
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Security.Principal;
    using BuildingBlocks;
    using EfCorePersistence;
    using Environment.DateAndTime;
    using Environment.MultiTenancy;
    using Patterns.Authorization;
    using Patterns.DependencyInjection;

    public class ServiceLocator
    {
        private readonly Assembly[] assemblies;

        public IClock Clock { get; }

        public ServiceLocator(IClock clock, params Assembly[] assemblies)
        {
            Clock = clock;
            this.assemblies = assemblies;
        }

        
        /// <summary>
        /// This naive auto wiring implementation assumes, that no other types beside IIdentity, IClock and IRepository are required by authorization classes
        /// </summary>
        /// <returns></returns>
        public IAggregateAuthorization<TAggregateRoot> GetAggregateAuthorization<TAggregateRoot>(
                EfUnitOfWork unitOfWork, 
                ICurrentTHolder<TenantId> tenantIdHolder, 
                ICurrentTHolder<IIdentity> identityHolder) where TAggregateRoot : AggregateRoot
        {
            return (IAggregateAuthorization<TAggregateRoot>)GetAggregateAuthorization(
                    unitOfWork, tenantIdHolder, identityHolder, typeof(TAggregateRoot));
        }
        
        public IRepository<TAggregateRoot> GetRepository<TAggregateRoot>(
                EfUnitOfWork unitOfWork, 
                ICurrentTHolder<TenantId> tenantIdHolder) where TAggregateRoot : AggregateRoot
        {
            return (IRepository<TAggregateRoot>)GetRepository(unitOfWork, tenantIdHolder, typeof(TAggregateRoot));
        }

        private object GetAggregateAuthorization(
                EfUnitOfWork unitOfWork,
                ICurrentTHolder<TenantId> tenantIdHolder, 
                ICurrentTHolder<IIdentity> identityHolder, 
                Type aggregateRootType)
        {
            Type aggregateDefinitionType = assemblies
                                           .SelectMany(ass => ass.GetTypes())
                                           .Where(t => t.GetTypeInfo().IsClass && !t.GetTypeInfo().IsAbstract)
                                           .SingleOrDefault(t => typeof(IAggregateAuthorization<>).MakeGenericType(aggregateRootType).GetTypeInfo().IsAssignableFrom(t));
            if (aggregateDefinitionType == null)
            {
                throw new InvalidOperationException(string.Format("No Aggregate authorization for {0} found", aggregateRootType.Name));
            }

            var constructorParameterTypes = aggregateDefinitionType.GetConstructors().Single().GetParameters();
            object[] constructorParameters = new object[constructorParameterTypes.Length];
            for (int i = 0; i < constructorParameterTypes.Length; i++)
            {
                if (constructorParameterTypes[i].ParameterType == typeof(IIdentity))
                {
                    constructorParameters[i] = identityHolder.Current;
                    continue;
                }

                if (constructorParameterTypes[i].ParameterType == typeof(ICurrentTHolder<IIdentity>))
                {
                    constructorParameters[i] = identityHolder;
                    continue;
                }

                if (constructorParameterTypes[i].ParameterType == typeof(IClock))
                {
                    constructorParameters[i] = Clock;
                    continue;
                }

                var genericTypeDefinition = constructorParameterTypes[i].ParameterType.GetGenericTypeDefinition();
                if (typeof(IRepository<>).IsAssignableFrom(genericTypeDefinition))
                {
                    constructorParameters[i] = GetRepository(unitOfWork, tenantIdHolder, constructorParameterTypes[i].ParameterType.GenericTypeArguments[0]);
                }
            }

            return Activator.CreateInstance(aggregateDefinitionType, constructorParameters);
        }

        public IAggregateMapping<TAggregateRoot> GetAggregateMapping<TAggregateRoot>() where TAggregateRoot : AggregateRoot
        {
            return (IAggregateMapping<TAggregateRoot>)GetAggregateMapping(typeof(TAggregateRoot));
        }

        private IAggregateMapping GetAggregateMapping(Type aggregateRootType)
        {
            Type aggregateDefinitionType = assemblies
                    .SelectMany(ass => ass.GetTypes())
                    .Where(t => IntrospectionExtensions.GetTypeInfo(t).IsClass && !IntrospectionExtensions.GetTypeInfo(t).IsAbstract)
                    .SingleOrDefault(t => typeof(IAggregateMapping<>).MakeGenericType(aggregateRootType).GetTypeInfo().IsAssignableFrom(t));
            if (aggregateDefinitionType == null)
            {
                throw new InvalidOperationException(string.Format("No Aggregate Definition for {0} found", aggregateRootType.Name));
            }

            return (IAggregateMapping)Activator.CreateInstance(aggregateDefinitionType);
        }

        private object GetRepository(
                EfUnitOfWork unitOfWork,
                ICurrentTHolder<TenantId> tenantIdHolder,
                Type aggregateRootType)
        {
            var aggregateAuthorization = GetAggregateAuthorization(unitOfWork, tenantIdHolder, unitOfWork.IdentityHolder, aggregateRootType);
            var efRepositoryType = typeof(EfRepository<>).MakeGenericType(aggregateRootType);
            return Activator.CreateInstance(efRepositoryType, unitOfWork.DbContext, GetAggregateMapping(aggregateRootType), tenantIdHolder, aggregateAuthorization);
        }
    }
}
