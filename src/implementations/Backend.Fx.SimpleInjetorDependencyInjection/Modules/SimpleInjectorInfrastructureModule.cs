using System;
using System.Reflection;
using Backend.Fx.Patterns.DependencyInjection;
using SimpleInjector;

namespace Backend.Fx.SimpleInjectorDependencyInjection.Modules
{
    public class SimpleInjectorInfrastructureModule : IInfrastructureModule
    {
        private readonly Container _container;

        public SimpleInjectorInfrastructureModule(Container container)
        {
            _container = container;
        }

        public void RegisterScoped<TService, TImpl>() where TService : class where TImpl : class, TService
        {
            _container.Register<TService, TImpl>();
        }

        public void RegisterScoped<TService>(Func<TService> factory) where TService : class
        {
            _container.Register(factory);
        }

        public void RegisterScoped(Type serviceType, Assembly[] assembliesToScan)
        {
            _container.Register(serviceType, assembliesToScan);
        }

        public void RegisterDecorator<TService, TImpl>() where TService : class where TImpl : class, TService
        {
            _container.RegisterDecorator<TService, TImpl>();
        }

        public void RegisterScoped(Type serviceType, Type implementationType)
        {
            _container.Register(serviceType, implementationType);
        }

        public void RegisterSingleton<TService, TImpl>() where TService : class where TImpl : class, TService
        {
            _container.RegisterSingleton<TService, TImpl>();
        }

        public void RegisterInstance<TService>(TService instance) where TService : class
        {
            _container.RegisterInstance(instance);
        }
    }
}
