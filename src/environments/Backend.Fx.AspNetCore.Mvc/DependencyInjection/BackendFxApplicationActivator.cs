using System;
using Backend.Fx.Patterns.DependencyInjection;

namespace Backend.Fx.AspNetCore.Mvc.DependencyInjection
{
    public abstract class BackendFxApplicationActivator
    {
        private readonly IBackendFxApplication _application;
        
        protected BackendFxApplicationActivator(IBackendFxApplication application)
        {
            _application = application;
        }

        protected object GetInstance(Type t)
        {
            return _application.CompositionRoot.InstanceProvider.GetInstance(t);
        }
    }
}
