namespace Backend.Fx.Patterns.DependencyInjection
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    
    public interface IScope : IDisposable
    {
        TService GetInstance<TService>() where TService : class;
        object GetInstance(Type serviceType);

        IEnumerable<TService> GetAllInstances<TService>() where TService : class;
        IEnumerable GetAllInstance(Type serviceType);
    }
}