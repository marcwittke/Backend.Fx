using Backend.Fx.Logging;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Backend.Fx.Patterns.DependencyInjection
{
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

        protected CurrentTHolder()
        { }

        protected CurrentTHolder(T initial)
        {
            _current = initial;
        }
        
        public T Current
        {
            get
            {
                if (_current == null)
                {
                    Logger.LogDebug("Providing initial {HeldTypeName} instance", typeof(T).Name);
                    _current = ProvideInstance();
                    Logger.LogDebug("Initial instance of {HeldTypeName} is: {HeldInstanceDescription}", typeof(T).Name, Describe(_current));
                }

                return _current;
            }
        }

        public void ReplaceCurrent(T newCurrentInstance)
        {
            if (Equals(_current, newCurrentInstance)) return;

            Logger.LogDebug(
                "Replacing current instance of {HeldTypename} ({HeldInstanceDescription}) with another instance ({NewInstanceDescription})",
                typeof(T).Name,
                Describe(Current),
                Describe(newCurrentInstance));
            _current = newCurrentInstance;
        }

        public abstract T ProvideInstance();

        protected abstract string Describe(T instance);
    }
}