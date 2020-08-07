using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Domain;
using SimpleInjector;
using SimpleInjector.Advanced;
using SimpleInjector.Lifestyles;

namespace Backend.Fx.SimpleInjectorDependencyInjection
{
    /// <summary>
    ///     Provides a reusable composition root assuming Simple Injector as container
    /// </summary>
    public class SimpleInjectorCompositionRoot : ICompositionRoot
    {
        private static readonly ILogger Logger = LogManager.Create<SimpleInjectorCompositionRoot>();

        private int _scopeSequenceNumber = 1;
        /// <summary>
        /// This constructor creates a composition root that prefers scoped lifestyle
        /// </summary>
        public SimpleInjectorCompositionRoot() 
            : this(new ScopedLifestyleBehavior(), new AsyncScopedLifestyle())
        {}

        public SimpleInjectorCompositionRoot(ILifestyleSelectionBehavior lifestyleBehavior, ScopedLifestyle scopedLifestyle)
        {
            Logger.Info("Initializing SimpleInjector");
            ScopedLifestyle = scopedLifestyle;
            Container.Options.LifestyleSelectionBehavior = lifestyleBehavior;
            Container.Options.DefaultScopedLifestyle = ScopedLifestyle;
        }

        public Container Container { get; } = new Container();

        internal ScopedLifestyle ScopedLifestyle { get; }

        public void RegisterModules(params IModule[] modules)
        {
            foreach (var module in modules)
            {
                Logger.Info($"Registering {module.GetType().Name}");
                module.Register(this);
            }
        }

        #region ICompositionRoot implementation

        public void Verify()
        {
            Logger.Info("container is being verified");
            try
            {
                Container.Verify(VerificationOption.VerifyAndDiagnose);
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "container configuration invalid");
                throw;
            }
        }


        public object GetInstance(Type serviceType)
        {
            return Container.GetInstance(serviceType);
        }

        public IEnumerable GetInstances(Type serviceType)
        {
            return Container.GetAllInstances(serviceType);
        }

        public T GetInstance<T>() where T : class
        {
            return Container.GetInstance<T>();
        }

        public IEnumerable<T> GetInstances<T>() where T : class
        {
            return Container.GetAllInstances<T>();
        }
        
        /// <inheritdoc />
        public IInjectionScope BeginScope()
        {
            return new SimpleInjectorInjectionScope(Interlocked.Increment(ref _scopeSequenceNumber), AsyncScopedLifestyle.BeginScope(Container));
        }

        public IInstanceProvider InstanceProvider
        {
            get { return new SimpleInjectorInstanceProvider(Container); }
        }

        public bool TryGetCurrentScope(out IInjectionScope currentScope)
        {
            throw new NotImplementedException();
        }

        public bool TryGetCurrentCorrelation(out Correlation correlation)
        {
            Scope scope = ScopedLifestyle.GetCurrentScope(Container);
            if (scope == null)
            {
                correlation = null;
                return false;
            }

            correlation = scope.GetInstance<ICurrentTHolder<Correlation>>().Current;
            return true;
        }

        public Scope GetCurrentScope()
        {
            return ScopedLifestyle.GetCurrentScope(Container);
        }
        #endregion

        #region IEventHandlerProvider implementation

        /// <inheritdoc />
        public IEnumerable<IDomainEventHandler<TDomainEvent>> GetAllEventHandlers<TDomainEvent>() where TDomainEvent : IDomainEvent
        {
            return Container.GetAllInstances<IDomainEventHandler<TDomainEvent>>();
        }
        #endregion

        #region IDisposable implementation
        public void Dispose()
        {
            Container?.Dispose();
        }
        #endregion

        /// <summary>
        ///     A behavior that defaults to scoped life style for injected instances
        /// </summary>
        private class ScopedLifestyleBehavior : ILifestyleSelectionBehavior
        {
            public Lifestyle SelectLifestyle(Type implementationType)
            {
                return Lifestyle.Scoped;
            }
        }
    }
}