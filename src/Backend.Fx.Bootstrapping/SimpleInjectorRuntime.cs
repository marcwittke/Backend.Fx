namespace Backend.Fx.Bootstrapping
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Security.Principal;
    using System.Threading.Tasks;
    using BuildingBlocks;
    using Environment.Authentication;
    using Environment.MultiTenancy;
    using Environment.Persistence;
    using Exceptions;
    using Logging;
    using Patterns.Authorization;
    using Patterns.DataGeneration;
    using Patterns.DependencyInjection;
    using Patterns.EventAggregation;
    using Patterns.Jobs;
    using Patterns.UnitOfWork;
    using SimpleInjector;
    using SimpleInjector.Advanced;
    using SimpleInjector.Lifestyles;

    /// <summary>
    /// Provides a reusable injection runtime assuming Simple Injector as injection 
    /// container and usage of Framework.Patterns (Event aggregation, Jobs, Unit of Work etc...)
    /// </summary>
    public abstract class SimpleInjectorRuntime : IRuntime
    {
        private static readonly ILogger Logger = LogManager.Create<SimpleInjectorRuntime>();
        private readonly EventAggregator eventAggregator;

        protected SimpleInjectorRuntime()
        {
            eventAggregator = new EventAggregator(this);
            Container.Options.LifestyleSelectionBehavior = new ScopedLifestyleBehavior();
            Container.Options.DefaultScopedLifestyle = ScopedLifestyle;
        }

        public abstract ITenantManager TenantManager { get; }

        public abstract IDatabaseManager DatabaseManager { get; }

        protected AsyncScopedLifestyle ScopedLifestyle { get; } = new AsyncScopedLifestyle();

        /// <summary>
        /// These assemblies are scanned for injected services.
        /// </summary>
        protected abstract Assembly[] Assemblies { get; }

        protected Container Container { get; } = new Container();

        #region bootstrapping
        public void Boot(Action<Container> bootAction = null)
        {
            using (Logger.InfoDuration("Booting application runtime", "Application runtime booted"))
            {
                BootFramework();
                BootPersistence();
                bootAction?.Invoke(Container);
                BootApplication();
                InitializeJobScheduler();
            }

            Verify();
        }


        protected virtual void BootFramework()
        {
            // the current IIdentity is resolved using the CurrentIdentityHolder that is maintained when opening a scope
            Container.Register<ICurrentTHolder<IIdentity>, CurrentIdentityHolder>();
            Container.Register(() => Container.GetInstance<ICurrentTHolder<IIdentity>>().Current);

            // same for the current tenant id
            Container.Register<ICurrentTHolder<TenantId>, CurrentTenantIdHolder>();
            Container.Register(() => Container.GetInstance<ICurrentTHolder<TenantId>>().Current);

            // same for scope interruption (use with care, described in detail here: https://codereview.stackexchange.com/questions/158534)
            Container.Register<ICurrentTHolder<IScopeInterruptor>, ScopeInterruptorHolder>();
            Container.Register(() => Container.GetInstance<ICurrentTHolder<IScopeInterruptor>>().Current);

            RegisterDomainAndApplicationServices();

            RegisterAuthorization();

            // domain event subsystem
            Container.RegisterSingleton<IEventAggregator>(eventAggregator);
            Container.RegisterCollection(typeof(IDomainEventHandler<>), Assemblies);

            // scheduled jobs subsystem
            foreach (var scheduledJobType in Container.GetTypesToRegister(typeof(IJob), Assemblies))
            {
                Container.Register(scheduledJobType);
            }

            // initial data generation subsystem
            Container.RegisterCollection<InitialDataGenerator>(Assemblies);
        }

        /// <summary>
        /// Auto registering all aggregate authorization classes
        /// </summary>
        private void RegisterAuthorization()
        {
            var aggregateRootAuthorizationTypes = Container.GetTypesToRegister(typeof(IAggregateAuthorization<>), Assemblies).ToArray();
            foreach (var aggregateRootAuthorizationType in aggregateRootAuthorizationTypes)
            {
                var serviceTypes = aggregateRootAuthorizationType
                    .GetTypeInfo()
                    .ImplementedInterfaces
                    .Where(impif => impif.GetTypeInfo().IsGenericType
                                     && impif.GenericTypeArguments.Length == 1
                                     && typeof(AggregateRoot).GetTypeInfo().IsAssignableFrom(impif.GenericTypeArguments[0].GetTypeInfo()));

                foreach (var serviceType in serviceTypes)
                {
                    Container.Register(serviceType, aggregateRootAuthorizationType);
                }
            }
        }

        /// <summary>
        /// Auto registering all implementors of <see cref="IApplicationService"/> and <see cref="IDomainService"/> with their implementations as scoped instances
        /// </summary>
        private void RegisterDomainAndApplicationServices()
        {
            var serviceRegistrations = Container
                    .GetTypesToRegister(typeof(IDomainService), Assemblies)
                    .Concat(Container.GetTypesToRegister(typeof(IApplicationService), Assemblies))
                    .SelectMany(type =>
                                    type.GetTypeInfo()
                                        .ImplementedInterfaces
                                        .Where(i => typeof(IDomainService) != i && typeof(IApplicationService) != i)
                                        .Select(service => new {
                                            Service = service,
                                            Implementation = type
                                        })
                    );
            foreach (var reg in serviceRegistrations)
            {
                Container.Register(reg.Service, reg.Implementation);
            }
        }

        /// <summary>
        /// wire the persistence services here. Also suitable for database creation/migration.
        /// </summary>
        protected abstract void BootPersistence();

        /// <summary>
        /// wire the services of your application here
        /// </summary>
        protected abstract void BootApplication();

        public void Verify()
        {
            Logger.Info("container is being verified");
            try
            {
                Container.Verify(VerificationOption.VerifyAndDiagnose);
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex, "container configuration invalid");
                throw;
            }
        }
        #endregion

        #region event subsystem
        /// <summary>
        /// Register a delegate that should be called asynchronously in a specific scope/transaction when the specific integration event is published.
        /// </summary>
        /// <param name="handler"></param>
        public void SubscribeToIntegrationEvent<T>(Action<T> handler) where T : IIntegrationEvent
        {
            eventAggregator.SubscribeToIntegrationEvent(handler);
        }

        public IEnumerable<IDomainEventHandler<TDomainEvent>> GetAllEventHandlers<TDomainEvent>() where TDomainEvent : IDomainEvent
        {
            return Container.GetAllInstances<IDomainEventHandler<TDomainEvent>>();
        }
        #endregion

        #region jobs subsystem

        protected abstract void InitializeJobScheduler();

        public async Task ExecuteJobAsync<TJob>(int? tenantId = null, int delayInSeconds = 0) where TJob : class, IJob
        {
            await Task.Delay(delayInSeconds * 1000);
            await Task.Run(() =>
            {
                TenantId[] tenants = tenantId == null
                                         ? TenantManager.GetTenantIds()
                                         : new[] { new TenantId(tenantId.Value) };

                string jobName = typeof(TJob).Name;
                foreach (var tenant in tenants)
                {
                    using (Logger.InfoDuration($"Beginning {jobName} scope", $"{jobName} scope completed"))
                    {
                        using (IRuntimeScope scope = BeginScope(new SystemIdentity(), tenant))
                        {
                            scope.BeginUnitOfWork(false);
                            try
                            {
                                scope.GetInstance<TJob>().Execute();
                                scope.GetInstance<IUnitOfWork>().Complete();
                            }
                            catch (Exception ex)
                            {
                                Logger.Error(ex, $"Execution of {jobName} failed: {ex.Message}");
                            }
                            finally
                            {
                                scope.GetInstance<IUnitOfWork>().Dispose();
                            }
                        }
                    }
                }
            });
        }
        #endregion

        #region container access
        public object GetInstance(Type serviceType)
        {
            return Container.GetInstance(serviceType);
        }

        public T GetInstance<T>() where T : class
        {
            return Container.GetInstance<T>();
        }
        #endregion

        #region scope handling
        /// <summary>
        /// Begins a new runtime injection scope for the given identity. The identity may be used 
        /// for authorization purposes inside the domain layer. The caller is responsable to complete
        /// the scope after finalization.
        /// </summary>
        public IRuntimeScope BeginScope(IIdentity identity, TenantId tenantId)
        {
            if (TenantManager.IsActive(tenantId))
            {
                var scope = new RuntimeScope(this, identity, tenantId);
                scope.GetInstance<ICurrentTHolder<IIdentity>>().ReplaceCurrent(identity);
                scope.GetInstance<ICurrentTHolder<TenantId>>().ReplaceCurrent(tenantId);
                return scope;
            }

            throw new UnprocessableException($"Tenant {tenantId.Value} is deactivated. Contact the system adminstrator.");
        }

        /// <summary>
        /// This should never be used in production code
        /// </summary>
        protected Scope GetCurrentScopeForTestsOnly()
        {
            return ScopedLifestyle.GetCurrentScope(Container);
        }
        
        private class RuntimeScope : IRuntimeScope
        {
            private readonly SimpleInjectorRuntime runtime;
            private Scope scope;
            private IUnitOfWork unitOfWork;
            private bool isReadonlyUnitOfWork;

            public RuntimeScope(SimpleInjectorRuntime runtime, IIdentity identity, TenantId tenantId)
            {
                LogManager.BeginActivity();
                this.runtime = runtime;
                scope = AsyncScopedLifestyle.BeginScope(runtime.Container);
                runtime.Container.GetInstance<ICurrentTHolder<IScopeInterruptor>>().ReplaceCurrent(this);
                Logger.Info($"Began new scope for identity [{identity.Name}] and tenant[{(tenantId.HasValue ? tenantId.Value.ToString() : "")}]");
            }

            public void BeginUnitOfWork(bool beginAsReadonlyUnitOfWork)
            {
                isReadonlyUnitOfWork = beginAsReadonlyUnitOfWork;

                unitOfWork = isReadonlyUnitOfWork
                    ? scope.Container.GetInstance<IReadonlyUnitOfWork>()
                    : scope.Container.GetInstance<IUnitOfWork>();

                unitOfWork.Begin();
            }

            public TService GetInstance<TService>() where TService : class
            {
                return scope.Container.GetInstance<TService>();
            }

            public void CompleteCurrentScope_InvokeAction_BeginNewScope(Action action)
            {
                Logger.Debug("Completing current scope to run an action without surrounding scope");
                var currentIdentity = scope.Container.GetInstance<ICurrentTHolder<IIdentity>>().Current;
                var currentTenantId = scope.Container.GetInstance<ICurrentTHolder<TenantId>>().Current;
                bool hasCurrentUnitOfWork = unitOfWork != null;
                if (hasCurrentUnitOfWork)
                {
                    unitOfWork.Complete();
                }
                scope.Dispose();
                scope = null;

                Logger.Info("Scope was completed to run an action without surrounding scope");
                try
                {
                    action.Invoke();
                }
                finally
                {
                    Logger.Debug("Beginning new scope after running action without surrounding scope");
                    scope = AsyncScopedLifestyle.BeginScope(runtime.Container);
                    scope.Container.GetInstance<ICurrentTHolder<IIdentity>>().ReplaceCurrent(currentIdentity);
                    scope.Container.GetInstance<ICurrentTHolder<TenantId>>().ReplaceCurrent(currentTenantId);
                    if (hasCurrentUnitOfWork)
                    {
                        BeginUnitOfWork(isReadonlyUnitOfWork);
                    }

                    Logger.Info($"Began new scope for identity [{currentIdentity.Name}] and tenant[{(currentTenantId.HasValue ? currentTenantId.Value.ToString() : "")}] after running action without surrounding scope");
                }
            }

            public T CompleteCurrentScope_InvokeFunction_BeginNewScope<T>(Func<T> func)
            {
                T result = default(T);
                CompleteCurrentScope_InvokeAction_BeginNewScope(() =>
                {
                    result = func.Invoke();
                });
                return result;
            }

            public void CompleteUnitOfWork()
            {
                unitOfWork.Complete();
            }

            public void Dispose()
            {
                // no need to dispose the encapsulated unit of work, since the container takes care of this when disposing the scope.
                // this is proven by unit tests
                scope?.Dispose();
                scope = null;
            }
        }

        /// <summary>
        /// A behavior that defaults to scoped life style for injected instances
        /// </summary>
        private class ScopedLifestyleBehavior : ILifestyleSelectionBehavior
        {
            public Lifestyle SelectLifestyle(Type implementationType)
            {
                return Lifestyle.Scoped;
            }
        }
        #endregion

        #region initial data generation
        public void RunProductiveInitialDataGenerators(TenantId tenantId)
        {
            Logger.Info("Loading productive data into database");
            RunInitialDataGeneratorsInSeparateScopes<IProductiveDataGenerator>(tenantId);
        }

        public void RunDemoInitialDataGenerators(TenantId tenantId)
        {
            Logger.Info("Loading demonstration data into database");
            RunInitialDataGeneratorsInSeparateScopes<IDemoDataGenerator>(tenantId);
        }

        private void RunInitialDataGeneratorsInSeparateScopes<T>(TenantId tenantId)
        {
            Queue<Type> toRun;
            using (BeginScope(new SystemIdentity(), tenantId))
            {
                toRun = new Queue<Type>(Container.GetAllInstances<InitialDataGenerator>().OrderBy(idg => idg.Priority).OfType<T>().Select(idg => idg.GetType()));
            }

            while (toRun.Any())
            {
                var type = toRun.Dequeue();
                using (IRuntimeScope scope = BeginScope(new SystemIdentity(), tenantId))
                {
                    scope.BeginUnitOfWork(false);
                    var instance = Container.GetAllInstances<InitialDataGenerator>().Single(idg => idg.GetType() == type);
                    instance.Generate();
                    scope.CompleteUnitOfWork();
                }
            }
        }
        #endregion

        #region IDisposable implementation
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Container?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}