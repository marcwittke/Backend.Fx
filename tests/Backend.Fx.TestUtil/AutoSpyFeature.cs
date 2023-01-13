using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.DependencyInjection;
using Backend.Fx.ExecutionPipeline;
using Backend.Fx.Features;
using Backend.Fx.Features.Authorization;
using Backend.Fx.Features.DomainEvents;
using Backend.Fx.Features.DomainServices;
using Backend.Fx.Features.MessageBus;
using Backend.Fx.Logging;
using Backend.Fx.SimpleInjectorDependencyInjection;
using Backend.Fx.Util;
using FakeItEasy;
using SimpleInjector;

namespace Backend.Fx.TestUtil
{

    public class AutoSpyFeature : Feature, IBootableFeature
    {
        private readonly AutoSpyModule _module = new();

        public Spies Spies => _module.Spies;

        public override void Enable(IBackendFxApplication application)
        {
            application.CompositionRoot.RegisterModules(_module);
        }

        public Task BootAsync(IBackendFxApplication application, CancellationToken cancellationToken = default)
        {
            Spies.ClearRecordedCalls();
            return Task.CompletedTask;
        }
    }

    public class AutoSpyModule : IModule
    {
        public Spies Spies { get; } = new();

        public void Register(ICompositionRoot compositionRoot)
        {
            Spies.AddAutoSpies(compositionRoot);
        }
    }

    public class Spies
    {
        private readonly ConcurrentDictionary<Type, ConcurrentStack<object>> _dynamicSpies = new();

        public IDomainEventAggregator DomainEventAggregator { get; } = A.Fake<IDomainEventAggregator>();

        public IMessageBusScope MessageBusScope { get; } = A.Fake<IMessageBusScope>();

        public IMessageBus MessageBus { get; } = A.Fake<IMessageBus>();

        public IExceptionLogger ExceptionLogger { get; } = A.Fake<IExceptionLogger>();

        public void ClearRecordedCalls()
        {
            lock (_dynamicSpies)
            {
                Fake.ClearRecordedCalls(DomainEventAggregator);
                Fake.ClearRecordedCalls(ExceptionLogger);
                Fake.ClearRecordedCalls(MessageBusScope);
                Fake.ClearRecordedCalls(MessageBus);

                foreach (object spy in _dynamicSpies.Values.SelectMany(o => o).ToArray())
                {
                    Fake.ClearRecordedCalls(spy);
                }
            }
        }

        /// <summary>
        /// Get the spy for a service as it was used in the last invocation
        /// </summary>
        public T LastOf<T>() where T : class
        {
            if (_dynamicSpies.TryGetValue(typeof(T), out ConcurrentStack<object> spies) && spies.TryPeek(out object spy))
            {
                return (T)spy;
            }

            throw new InvalidOperationException($"No spy for {typeof(T).Name} available");
        }

        /// <summary>
        /// Get the spies for a service from all invocations
        /// </summary>
        public T[] AllOf<T>()
        {
            if (_dynamicSpies.TryGetValue(typeof(T), out ConcurrentStack<object> spies))
            {
                return spies.Cast<T>().ToArray();
            }

            throw new InvalidOperationException($"No spies for {typeof(T).Name} available");
        }

        public void AddAutoSpies(ICompositionRoot compositionRoot)
        {
            Container container = ((SimpleInjectorCompositionRoot)compositionRoot).Container;
            container.ExpressionBuilt +=
                (_, args) =>
                {
                    // we only spy on interfaces
                    if (!args.RegisteredServiceType.IsInterface) return;

                    // Settings services do have a name semantic that breaks when wrapping it with a proxy
                    if (args.RegisteredServiceType.Name.EndsWith("Settings")) return;

                    // we spy only on our common domain interfaces, otherwise some casts are broken
                    var shouldSpyOnIt = false;
                    shouldSpyOnIt |= IsDomainEventHandler(args.RegisteredServiceType);
                    shouldSpyOnIt |= IsAuthorizationPolicy(args.RegisteredServiceType);
                    shouldSpyOnIt |= IsDomainService(args.RegisteredServiceType);
                    shouldSpyOnIt |= IsApplicationService(args.RegisteredServiceType);
                    shouldSpyOnIt |= typeof(IOperation).IsAssignableFrom(args.RegisteredServiceType);

                    if (shouldSpyOnIt)
                    {
                        // Compile the original object creation into a delegate.
                        var factory = (Func<object>)Expression.Lambda(typeof(Func<object>), args.Expression).Compile();

                        // Create a registration for the spy, based on the original lifestyle
                        Registration spyRegistration = args.Lifestyle.CreateRegistration(
                            args.RegisteredServiceType,
                            () =>
                            {
                                object instance = factory();

                                // we're using the non generic fake creation as implemented in https://github.com/FakeItEasy/FakeItEasy/issues/482#issuecomment-227122045
                                // so we get an object proxy that:
                                // - implements the service type interface
                                // - provides call recording and assertion
                                // - delegates to the original service type implementation
                                object spy = FakeItEasy.Sdk.Create.Fake(
                                    args.RegisteredServiceType,
                                    options => options.Wrapping(instance).Implements(args.RegisteredServiceType));
                                Register(args.RegisteredServiceType, spy);
                                return spy;
                            },
                            container);

                        // Replace expression of the registration with the spy registration.
                        args.Expression = spyRegistration.BuildExpression();
                    }
                };
        }

        private static bool IsDomainEventHandler(Type type) => ImplementsInterfaceOrExistsTypeThatImplementsIt(typeof(IDomainEventHandler<>), type);

        private static bool IsAuthorizationPolicy(Type type) => ImplementsInterfaceOrExistsTypeThatImplementsIt(typeof(IAuthorizationPolicy<>), type);

        private static bool IsDomainService(Type type) => ImplementsInterfaceOrExistsTypeThatImplementsIt<IDomainService>(type);

        private static bool IsApplicationService(Type type) => ImplementsInterfaceOrExistsTypeThatImplementsIt<IApplicationService>(type);

        private static bool ImplementsInterfaceOrExistsTypeThatImplementsIt<TInterface>(Type serviceType)
            => ImplementsInterfaceOrExistsTypeThatImplementsIt(typeof(TInterface), serviceType);

        private static bool ImplementsInterfaceOrExistsTypeThatImplementsIt(Type interfaceType, Type serviceType)
        {
            if (interfaceType.IsOpenGeneric() && serviceType.IsGenericType)
            {
                if (serviceType.IsGenericType && interfaceType.IsAssignableFrom(serviceType.GetGenericTypeDefinition()))
                {
                    return true;
                }
            }

            var result = interfaceType.IsAssignableFrom(serviceType) ||
                         serviceType.Assembly.GetTypes().Any(t => t.IsClass && !t.IsAbstract && interfaceType.IsAssignableFrom(t));

            return result;
        }

        private void Register(Type serviceType, object spy)
        {
            if (spy == null) throw new ArgumentNullException(nameof(spy));
            _dynamicSpies.AddOrUpdate(
                serviceType,
                _ =>
                {
                    var stack = new ConcurrentStack<object>();
                    stack.Push(spy);
                    return stack;
                },
                (_, list) =>
                {
                    list.Push(spy);
                    return list;
                });
        }
    }
}