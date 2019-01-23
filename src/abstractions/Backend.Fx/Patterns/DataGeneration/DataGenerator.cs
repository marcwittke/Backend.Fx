namespace Backend.Fx.Patterns.DataGeneration
{
    using Logging;

    /// <summary>
    /// Implement this abstract class and mark it either with the <see cref="IDemoDataGenerator"/> 
    /// or <see cref="IProductiveDataGenerator"/> depending whether you want it to run in all environments
    /// or only on development environments.
    /// Any implementation is automatically picked up by the injection container, so no extra plumbing is required.
    /// You can require any application or domain service including repositories via constructor parameter.
    /// </summary>
    public abstract class DataGenerator
    {
        private static readonly ILogger Logger = LogManager.Create<DataGenerator>();

        /// <summary>
        /// simple way of ordering the execution of DataGenerators. Priority 0 will be executed first.
        /// </summary>
        public abstract int Priority { get; }

        public void Generate()
        {
            if (ShouldRun())
            {
                Initialize();
                Logger.Info($"{GetType().FullName} is now generating initial data");
                GenerateCore();
            }
        }

        /// <summary>
        /// Implement your generate Logic here
        /// </summary>
        protected abstract void GenerateCore();

        /// <summary>
        /// Implement your initial logic here (e.g. loading from external source)
        /// </summary>
        protected abstract void Initialize();

        /// <summary>
        /// return true, if the generator should be executed. Generators must be implemented idempotent, 
        /// since they're all executed on application start
        /// </summary>
        /// <returns></returns>
        protected abstract bool ShouldRun();
    }
}