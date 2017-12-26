namespace Backend.Fx.Exceptions
{
    using System;
    using Logging;

    public class UnprocessableExceptionBuilder : IDisposable
    {
        private const string EmptyKey = "";
        private static readonly ILogger Logger = LogManager.Create<UnprocessableExceptionBuilder>();
        private readonly UnprocessableException unprocessableException = new UnprocessableException();

        internal UnprocessableExceptionBuilder()
        { }

        public void Add(string error)
        {
            unprocessableException.AddError(EmptyKey, error);
        }

        public void Add(string key, string error)
        {
            unprocessableException.AddError(key, error);
        }

        public void AddNotFoundWhenNull<T>(object id, T t)
        {
            if (t == null)
            {
                unprocessableException.AddError(EmptyKey, $"{typeof(T).Name} [{id}] not found");
            }
        }

        public void AddNotFoundWhenNull<T>(string key, object id, T t)
        {
            if (t == null)
            {
                unprocessableException.AddError(key, $"{typeof(T).Name} [{id}] not found");
            }
        }

        public void Dispose()
        {
            if (unprocessableException.HasErrors)
            {
                throw new UnprocessableException("The provided arguments cannot be processed");
            }
        }

        public void AddIf(bool condition, string error)
        {
            if (condition)
            {
                unprocessableException.AddError(EmptyKey, error);
            }
        }

        public void AddIf(string key, bool condition, string error)
        {
            if (condition)
            {
                unprocessableException.AddError(key, error);
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
                unprocessableException.AddError(EmptyKey, ex.Message);
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
                unprocessableException.AddError(key, ex.Message);
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
                unprocessableException.AddError(EmptyKey, ex.Message);
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
                unprocessableException.AddError(key, ex.Message);
            }
        }
    }
}