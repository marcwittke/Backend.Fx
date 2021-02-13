using System;

namespace Backend.Fx.Extensions
{
    public class MultipleDisposable : IDisposable
    {
        private readonly IDisposable[] _disposables;

        public MultipleDisposable(params IDisposable[] disposables)
        {
            _disposables = disposables;
        }

        public void Dispose()
        {
            foreach (IDisposable disposable in _disposables)
            {
                disposable?.Dispose();
            }
        }
    }
}