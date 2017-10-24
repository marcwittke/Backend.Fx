namespace Backend.Fx.Patterns.DependencyInjection
{
    /// <summary>
    /// Initializable instances must be singletons
    /// </summary>
    public interface IInitializable
    {
        /// <summary>
        /// The initialize method is called during Boot of the <see cref="BackendFxApplication"/>
        /// </summary>
        void Initialize();
    }
}
