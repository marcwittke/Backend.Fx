using System;
using System.Collections;
using System.Collections.Generic;
using Backend.Fx.Patterns.DependencyInjection;
using SimpleInjector;

namespace Backend.Fx.SimpleInjectorDependencyInjection
{
    public class SimpleInjectorInstanceProvider : IInstanceProvider
    {
        private readonly Container _container;

        public SimpleInjectorInstanceProvider(Container container)
        {
            _container = container;
        }

        public object GetInstance(Type serviceType)
        {
            return _container.GetInstance(serviceType);
        }

        public IEnumerable GetInstances(Type serviceType)
        {
            return (IEnumerable)_container.GetInstance(typeof(IEnumerable<>).MakeGenericType(serviceType));
        }

        public T GetInstance<T>() where T : class
        {
            return _container.GetInstance<T>();
        }

        public IEnumerable<T> GetInstances<T>() where T : class
        {
            return _container.GetInstance<IEnumerable<T>>();
        }
    }
}
