namespace Backend.Fx.Exceptions
{
    using System;
    using Logging;

    public interface IExceptionBuilder : IDisposable
    {
        void Add(Error error);
        void Add(string key, Error error);
        void AddNotFoundWhenNull<T>(object id, T t);
        void AddNotFoundWhenNull<T>(string key, object id, T t);
        void AddIf(bool condition, Error error);
        void AddIf(string key, bool condition, Error error);
        T CatchPossibleException<T>(Func<T> function);
        T CatchPossibleException<T>(string key, Func<T> function);
        void CatchPossibleException(Action action);
        void CatchPossibleException(string key, Action action);
    }

    public class ExceptionBuilder<TEx> : IExceptionBuilder where TEx : ClientException, new()
    {
        private static readonly ILogger Logger = LogManager.Create<ExceptionBuilder<TEx>>();
        private readonly TEx _clientException = new TEx();

        public void Add(Error error)
        {
            _clientException.Errors.Add(Errors.GenericErrorKey, error);
        }

        public void Add(string key, Error error)
        {
            _clientException.Errors.Add(key, error);
        }

        public void AddNotFoundWhenNull<T>(object id, T t)
        {
            if (t == null)
            {
                _clientException.Errors.Add(Errors.GenericErrorKey, new Error("NotFound", $"{typeof(T).Name} [{id}] not found"));
            }
        }

        public void AddNotFoundWhenNull<T>(string key, object id, T t)
        {
            if (t == null)
            {
                _clientException.Errors.Add(key, new Error("NotFound", $"{typeof(T).Name} [{id}] not found"));
            }
        }

        public void Dispose()
        {
            if (_clientException.HasErrors())
            {
                throw _clientException;
            }
        }

        public void AddIf(bool condition, Error error)
        {
            if (condition)
            {
                _clientException.Errors.Add(Errors.GenericErrorKey, error);
            }
        }

        public void AddIf(string key, bool condition, Error error)
        {
            if (condition)
            {
                _clientException.Errors.Add(key, error);
            }
        }

        public T CatchPossibleException<T>(Func<T> function)
        {
            T t = default(T);
            try
            {
                t = function();
            }
            catch (Exception ex)
            {
                Logger.Info(ex, $"Exception of type {ex.GetType().Name} will be appended to an {nameof(UnprocessableException)}.");
                _clientException.Errors.Add(Errors.GenericErrorKey, new Error(ex.GetType().Name, ex.Message));
            }
            return t;
        }

        public T CatchPossibleException<T>(string key, Func<T> function)
        {
            T t = default(T);
            try
            {
                t = function();
            }
            catch (Exception ex)
            {
                Logger.Info(ex, $"Exception of type {ex.GetType().Name} will be appended to an {nameof(UnprocessableException)}.");
                _clientException.Errors.Add(key, new Error(ex.GetType().Name, ex.Message));
            }
            return t;
        }

        public void CatchPossibleException(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Logger.Info(ex, $"Exception of type {ex.GetType().Name} will be appended to an {nameof(UnprocessableException)}.");
                _clientException.Errors.Add(Errors.GenericErrorKey, new Error(ex.GetType().Name, ex.Message));
            }
        }

        public void CatchPossibleException(string key, Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Logger.Info(ex, $"Exception of type {ex.GetType().Name} will be appended to an {nameof(UnprocessableException)}.");
                _clientException.Errors.Add(key, new Error(ex.GetType().Name, ex.Message));
            }
        }
    }
}