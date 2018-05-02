namespace Backend.Fx.Exceptions
{
    using System;
    using Logging;

    public class ExceptionBuilder<TEx> : IDisposable where TEx : ClientException, new()
    {
        private static readonly ILogger Logger = LogManager.Create<ExceptionBuilder<TEx>>();
        private readonly TEx clientException = new TEx();

        public ExceptionBuilder()
        { }

        public void Add(Error error)
        {
            clientException.Errors.Add(Errors.GenericErrorKey, error);
        }

        public void Add(string key, Error error)
        {
            clientException.Errors.Add(key, error);
        }

        public void AddNotFoundWhenNull<T>(object id, T t)
        {
            if (t == null)
            {
                clientException.Errors.Add(Errors.GenericErrorKey, new Error("NotFound", $"{typeof(T).Name} [{id}] not found"));
            }
        }

        public void AddNotFoundWhenNull<T>(string key, object id, T t)
        {
            if (t == null)
            {
                clientException.Errors.Add(key, new Error("NotFound", $"{typeof(T).Name} [{id}] not found"));
            }
        }

        public void Dispose()
        {
            if (clientException.HasErrors())
            {
                throw clientException;
            }
        }

        public void AddIf(bool condition, Error error)
        {
            if (condition)
            {
                clientException.Errors.Add(Errors.GenericErrorKey, error);
            }
        }

        public void AddIf(string key, bool condition, Error error)
        {
            if (condition)
            {
                clientException.Errors.Add(key, error);
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
                clientException.Errors.Add(Errors.GenericErrorKey, new Error(ex.GetType().Name, ex.Message));
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
                clientException.Errors.Add(key, new Error(ex.GetType().Name, ex.Message));
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
                clientException.Errors.Add(Errors.GenericErrorKey, new Error(ex.GetType().Name, ex.Message));
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
                clientException.Errors.Add(key, new Error(ex.GetType().Name, ex.Message));
            }
        }
    }

    public class UnprocessableExceptionBuilder : ExceptionBuilder<UnprocessableException> { }

    public class ClientExceptionBuilder : ExceptionBuilder<ClientException> { }
}