using System;
using Backend.Fx.Patterns.DependencyInjection;

namespace Backend.Fx.AspNetCore.Mvc.DependencyInjection
{
    public abstract class CompositionRootActivator
    {
        private readonly ICompositionRoot _compositionRoot;
        
        protected CompositionRootActivator(ICompositionRoot compositionRoot)
        {
            this._compositionRoot = compositionRoot;
        }

        public object GetInstance(Type t)
        {
            return _compositionRoot.GetInstance(t);
        }
    }
}
