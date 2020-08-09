namespace Backend.Fx.Patterns.DependencyInjection
{
    public interface IModule
    {
        void Register(ICompositionRoot compositionRoot);
    }
}