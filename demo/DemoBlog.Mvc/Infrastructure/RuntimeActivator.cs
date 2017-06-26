namespace DemoBlog.Mvc.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using Backend.Fx.Patterns.DependencyInjection;

    public abstract class RuntimeActivator
    {
        private readonly IRuntime runtime;
        private readonly Dictionary<Type, Func<object>> frameworkOnlyFactories = new Dictionary<Type, Func<object>>();

        protected RuntimeActivator(IRuntime runtime)
        {
            this.runtime = runtime;
        }

        public object GetInstance(Type t)
        {
            if (frameworkOnlyFactories.ContainsKey(t))
            {
                return frameworkOnlyFactories[t].Invoke();
            }

            return runtime.GetInstance(t);
        }

        public void RegisterFrameworkOnlyService<T>(Func<T> factory)
        {
            frameworkOnlyFactories.Add(typeof(T), () => factory.Invoke());
        }
    }
}
