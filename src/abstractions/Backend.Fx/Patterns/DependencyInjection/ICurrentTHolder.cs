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

        T ProvideInstance();
    }

    public abstract class CurrentTHolder<T> : ICurrentTHolder<T> where T : class
    {
        private static readonly ILogger Logger = LogManager.Create<CurrentTHolder<T>>();
        private T _current;

        public T Current
        {
            get
            {
                if (_current == null)
                {
                    Logger.Debug($"Providing initial {typeof(T).Name} instance");
                    _current = ProvideInstance();
                    Logger.Debug($"Initial instance of {typeof(T).Name} is: {Describe(_current)}");
                }
                return _current;
            }
        }

        public void ReplaceCurrent(T newCurrentInstance)
        {
            if (Equals(_current, newCurrentInstance)) return;

            Logger.Debug($"Replacing current instance of {typeof(T).Name} ({Describe(Current)}) with another instance ({Describe(newCurrentInstance)})");
            _current = newCurrentInstance;
        }

        public abstract T ProvideInstance();

        protected abstract string Describe(T instance);
    }
}
