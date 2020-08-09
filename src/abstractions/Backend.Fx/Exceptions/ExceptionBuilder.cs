using System;

namespace Backend.Fx.Exceptions
{
    public interface IExceptionBuilder : IDisposable
    {
        void Add(string error);
        void Add(string key, string error);
        void AddNotFoundWhenNull<T>(object id, T t);
        void AddIf(bool condition, string error);
        void AddIf(string key, bool condition, string error);
    }

    public class ExceptionBuilder<TEx> : IExceptionBuilder where TEx : ClientException, new()
    {
        private readonly TEx _clientException = new TEx();

        public void Add(string error)
        {
            _clientException.Errors.Add(error);
        }

        public void Add(string key, string error)
        {
            _clientException.Errors.Add(key, error);
        }

        public void AddNotFoundWhenNull<T>(object id, T t)
        {
            if (t == null)
            {
                _clientException.Errors.Add("NotFound", $"{typeof(T).Name} [{id}] not found");
            }
        }

        public void Dispose()
        {
            if (_clientException.HasErrors())
            {
                throw _clientException;
            }
        }

        public void AddIf(bool condition, string error)
        {
            if (condition)
            {
                _clientException.Errors.Add(error);
            }
        }

        public void AddIf(string key, bool condition, string error)
        {
            if (condition)
            {
                _clientException.Errors.Add(key, error);
            }
        }
    }
}