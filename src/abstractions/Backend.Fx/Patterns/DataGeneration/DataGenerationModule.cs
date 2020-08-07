using Backend.Fx.Patterns.DependencyInjection;

namespace Backend.Fx.Patterns.DataGeneration
{
    public abstract class DataGenerationModule : IModule
    {
        public void Register(ICompositionRoot compositionRoot)
        {
            RegisterDataGenerators(compositionRoot);
        }

        protected abstract void RegisterDataGenerators(ICompositionRoot compositionRoot);
    }
}