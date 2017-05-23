namespace Backend.Fx.Patterns.DependencyInjection
{
    using Logging;

    /// <summary>
    /// Holds a current instance of T that might be replaced during the scope
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICurrentTHolder<T> where T : class
    {
        T Current { get; }

        void ReplaceCurrent(T newCurrentInstance);

        T ProvideInitialInstance();
    }

    public abstract class CurrentTHolder<T> : ICurrentTHolder<T> where T : class
    {
        private static readonly ILogger Logger = LogManager.Create<CurrentTHolder<T>>();
        private T current;

        public T Current
        {
            get
            {
                if (current == null)
                {
                    Logger.Debug($"Providing initial {typeof(T).Name} instance");
                    current = ProvideInitialInstance();
                    Logger.Debug($"Initial instance of {typeof(T).Name} is: {Describe(current)}");
                }
                return current;
            }
        }

        public void ReplaceCurrent(T newCurrentInstance)
        {
            if (Equals(current, newCurrentInstance)) return;

            Logger.Debug($"Replacing current instance of {typeof(T).Name} ({Describe(Current)}) with another instance ({Describe(newCurrentInstance)})");
            current = newCurrentInstance;
        }

        public abstract T ProvideInitialInstance();

        protected abstract string Describe(T instance);
    }
}
