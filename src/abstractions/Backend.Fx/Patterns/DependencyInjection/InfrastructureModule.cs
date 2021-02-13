using System;
using System.Reflection;

namespace Backend.Fx.Patterns.DependencyInjection
{
    public interface IInfrastructureModule
    {
        void RegisterScoped<TService, TImpl>()
            where TImpl : class, TService
            where TService : class;
        
        void RegisterScoped<TService>(Func<TService> factory)
            where TService : class;

        void RegisterScoped(Type serviceType, Type implementationType);
        void RegisterScoped(Type serviceType, Assembly[] assembliesToScan);

        void RegisterDecorator<TService, TImpl>() where TService : class where TImpl : class, TService;

        void RegisterSingleton<TService, TImpl>() where TService : class where TImpl : class, TService;
        void RegisterInstance<TService>(TService instance) where TService : class;
    }
}