using System;

namespace Backend.Fx.Patterns.DependencyInjection
{
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