using System;
using JetBrains.Annotations;

namespace Backend.Fx.Features
{
    /// <summary>
    /// Base class for optional features that can be added to the Backend.Fx execution pipeline
    /// </summary>
    [PublicAPI]
    public abstract class Feature : IDisposable
    {
        public abstract void Enable(IBackendFxApplication application);

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}