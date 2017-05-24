namespace Backend.Fx.Exceptions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Logging;

    public class UnprocessableExceptionBuilder : IDisposable
    {
        private static readonly ILogger Logger = LogManager.Create<UnprocessableExceptionBuilder>();
        private readonly List<string> errors = new List<string>();

        internal UnprocessableExceptionBuilder()
        { }

        public void Add(string error)
        {
            errors.Add(error);
        }

        public void AddNotFoundWhenNull<T>(int id, T t)
        {
            if (t == null)
            {
                errors.Add($"{typeof(T).Name} [{id}] not found");
            }
        }

        public void Dispose()
        {
            if (errors.Any())
            {
                throw new UnprocessableException("The provided arguments cannot be processed: " + string.Join(", ", errors));
            }
        }

        public void AddIf(bool condition, string error)
        {
            if (condition)
            {
                errors.Add(error);
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
                errors.Add(ex.Message);
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
                errors.Add(ex.Message);
            }
        }
    }
}