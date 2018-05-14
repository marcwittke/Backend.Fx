namespace Backend.Fx.AspNetCore
{
    using System;
    using Patterns.DependencyInjection;

    public abstract class CompositionRootActivator
    {
        private readonly ICompositionRoot compositionRoot;
        
        protected CompositionRootActivator(ICompositionRoot compositionRoot)
        {
            this.compositionRoot = compositionRoot;
        }

        public object GetInstance(Type t)
        {
            return compositionRoot.GetInstance(t);
        }
    }
}
