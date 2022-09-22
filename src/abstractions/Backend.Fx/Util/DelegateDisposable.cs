using System;
using JetBrains.Annotations;

namespace Backend.Fx.Util
{
    public class DelegateDisposable : IDisposable
    {
        private readonly Action _onDisposal;

        public DelegateDisposable([NotNull] Action onDisposal)
        {
            _onDisposal = onDisposal ?? throw new ArgumentNullException(nameof(onDisposal));
        }

        public void Dispose()
        {
            _onDisposal();
        }
    }
}