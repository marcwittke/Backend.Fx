using System;

namespace Backend.Fx.Patterns.DependencyInjection
{
    /// <summary>
    /// During a scope, services by default are singletons. Scopes may exist in parallel, providing totally separate singleton
    /// instances for every scope.
    /// </summary>
    public interface IInjectionScope : IDisposable
    {
        int SequenceNumber { get; }

        IInstanceProvider InstanceProvider { get; }
    }


    public abstract class InjectionScope : IInjectionScope
    {
        protected InjectionScope(int sequenceNumber)
        {
            SequenceNumber = sequenceNumber;
        }

        public int SequenceNumber { get; }

        public abstract IInstanceProvider InstanceProvider { get; }

        public abstract void Dispose();
    }
}
