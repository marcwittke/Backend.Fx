using System;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using Backend.Fx.BuildingBlocks;
using Backend.Fx.Environment.Authentication;
using Backend.Fx.Environment.DateAndTime;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Environment.Persistence;
using Backend.Fx.Patterns.Authorization;
using Backend.Fx.Patterns.EventAggregation.Domain;
using Backend.Fx.Patterns.EventAggregation.Integration;
using Backend.Fx.RandomData;

namespace Backend.Fx.Patterns.DependencyInjection.Pure
{
    /// <summary>
    /// A strongly typed registry of instances available during a scope, without the need for a dependency injection container.
    /// </summary>
    /// <remarks>This pattern is called "Pure DI", see https://blog.ploeh.dk/2012/11/06/WhentouseaDIContainer/ for details.
    /// It is very useful in testing scenarios.</remarks>
    public interface IScopedServices : IDisposable
    {
        ICanFlush CanFlush { get; }
        AdjustableClock Clock { get; }
        IDomainEventAggregator EventAggregator { get; }
        IMessageBusScope MessageBusScope { get; }
        ICurrentTHolder<IIdentity> IdentityHolder { get; }
        ICurrentTHolder<TenantId> TenantIdHolder { get; }
        ICurrentTHolder<Correlation> CorrelationHolder { get; }
        TenantId TenantId { get; }
    }

    public abstract class ScopedServices : IScopedServices
    {
        private readonly Assembly[] _domainAssemblies;
        private readonly Lazy<ICanFlush> _canFlush;
        private bool _doAutoFlush = true;

        protected ScopedServices(
            IClock clock,
            IIdentity identity,
            TenantId tenantId,
            params Assembly[] domainAssemblies)
        {
            _domainAssemblies = domainAssemblies;
            Clock = new AdjustableClock(new FrozenClock(clock));
            TenantIdHolder = CurrentTenantIdHolder.Create(tenantId);
            IdentityHolder = CurrentIdentityHolder.Create(identity);
            CorrelationHolder = new CurrentCorrelationHolder();
            _canFlush = new Lazy<ICanFlush>(CreateCanFlush);
        }

        public ICanFlush CanFlush => _canFlush.Value;

        public ICurrentTHolder<TenantId> TenantIdHolder { get; }

        public ICurrentTHolder<Correlation> CorrelationHolder { get; }

        public ICurrentTHolder<IIdentity> IdentityHolder { get; }

        public AdjustableClock Clock { get; }

        public abstract IDomainEventAggregator EventAggregator { get; }

        public abstract IMessageBusScope MessageBusScope { get; }

        public TenantId TenantId => TenantIdHolder.Current;

        public ScopedServices OptOutOfAutoFlush()
        {
            _doAutoFlush = false;
            return this;
        }

        public T AddToRepository<T>(T aggregateRoot) where T : AggregateRoot
        {
            GetRepository<T>().Add(aggregateRoot);
            Flush();
            return aggregateRoot;
        }

        public T Get<T>(int id) where T : AggregateRoot
        {
            return GetRepository<T>().Single(id);
        }

        public abstract IRepository<TAggregateRoot> GetRepository<TAggregateRoot>() where TAggregateRoot : AggregateRoot;

        public abstract IAsyncRepository<TAggregateRoot> GetAsyncRepository<TAggregateRoot>() where TAggregateRoot : AggregateRoot;

        public T GetRandom<T>() where T : AggregateRoot
        {
            return GetRepository<T>().AggregateQueryable.Random();
        }

        public void Flush()
        {
            CanFlush.Flush();
        }

        public void Complete()
        {
            Flush();
        }

        public void Dispose()
        {
            if (_doAutoFlush)
            {
                Flush();
            }

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected object GetAggregateAuthorization(ICurrentTHolder<IIdentity> identityHolder, Type aggregateRootType)
        {
            Type aggregateDefinitionType = _domainAssemblies
                                           .SelectMany(ass => ass.GetTypes())
                                           .Where(t => t.GetTypeInfo().IsClass && !t.GetTypeInfo().IsAbstract)
                                           .SingleOrDefault(t =>
                                               typeof(IAggregateAuthorization<>).MakeGenericType(aggregateRootType).GetTypeInfo()
                                                                                .IsAssignableFrom(t.GetTypeInfo()));
            if (aggregateDefinitionType == null)
            {
                throw new InvalidOperationException($"No Aggregate authorization for {aggregateRootType.Name} found");
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

                constructorParameterTypes[i] = ProvideInstance(constructorParameterTypes[i].ParameterType);

                if (constructorParameterTypes[i] == null)
                {
                    throw new InvalidOperationException($"No implementation for {constructorParameterTypes[i].ParameterType.Name} provided");
                }
            }

            return Activator.CreateInstance(aggregateDefinitionType, constructorParameters);
        }

        protected virtual ParameterInfo ProvideInstance(Type parameterType)
        {
            return null;
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        protected abstract ICanFlush CreateCanFlush();
    }
}