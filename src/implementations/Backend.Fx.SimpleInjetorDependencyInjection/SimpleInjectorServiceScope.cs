using System;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;

namespace Backend.Fx.SimpleInjectorDependencyInjection
{
    public class SimpleInjectorServiceScope : IServiceScope
    {
        private readonly Scope _scope;

        public SimpleInjectorServiceScope(Scope scope)
        {
            _scope = scope;
        }

        public IServiceProvider ServiceProvider => _scope.Container;


        public void Dispose()
        {
            _scope.Dispose();
        }
    }
}