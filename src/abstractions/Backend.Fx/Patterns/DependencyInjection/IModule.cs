namespace Backend.Fx.Patterns.DependencyInjection
{
    /// <summary>
    /// A logically cohesive bunch of services
    /// </summary>
    public interface IModule
    {
        void Register(ICompositionRoot compositionRoot);
    }
}