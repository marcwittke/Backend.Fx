namespace DemoBlog.Mvc.Infrastructure.Integration
{
    using System;
    using System.Collections.Generic;
    using Backend.Fx.Patterns.DependencyInjection;

    public abstract class CompositionRootActivator
    {
        private readonly ICompositionRoot compositionRoot;
        private readonly Dictionary<Type, Func<object>> frameworkOnlyFactories = new Dictionary<Type, Func<object>>();

        protected CompositionRootActivator(ICompositionRoot compositionRoot)
        {
            this.compositionRoot = compositionRoot;
        }

        public object GetInstance(Type t)
        {
            if (frameworkOnlyFactories.ContainsKey(t))
            {
                return frameworkOnlyFactories[t].Invoke();
            }

            return compositionRoot.GetInstance(t);
        }

        public void RegisterFrameworkOnlyService<T>(Func<T> factory)
        {
            frameworkOnlyFactories.Add(typeof(T), () => factory.Invoke());
        }
    }
}
