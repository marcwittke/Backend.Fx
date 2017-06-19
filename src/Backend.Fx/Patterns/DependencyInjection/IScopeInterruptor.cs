namespace Backend.Fx.Patterns.DependencyInjection
{
    using System;

    /// <summary>
    /// Require this class in your constructor, if you require an action to run outside a inejction scope
    /// Normally this is not needed, but there are exceptions.
    /// </summary>
    public interface IScopeInterruptor
    {
        void CompleteCurrentScope_InvokeAction_BeginNewScope(Action action);

        T CompleteCurrentScope_InvokeFunction_BeginNewScope<T>(Func<T> func);

        void CompleteCurrentScope_BeginNewScope();
    }

    internal class NullScopeInterruptor : IScopeInterruptor
    {
        public void CompleteCurrentScope_InvokeAction_BeginNewScope(Action action)
        {
            throw new InvalidOperationException("You are not inside a scope that could be interrupted");
        }

        public T CompleteCurrentScope_InvokeFunction_BeginNewScope<T>(Func<T> func)
        {
            throw new InvalidOperationException("You are not inside a scope that could be interrupted");
        }

        public void CompleteCurrentScope_BeginNewScope()
        {
            throw new InvalidOperationException("You are not inside a scope that could be interrupted");
        }
    }

    public class ScopeInterruptorHolder : CurrentTHolder<IScopeInterruptor>
    {
        public override IScopeInterruptor ProvideInitialInstance()
        {
            return new NullScopeInterruptor();
        }

        protected override string Describe(IScopeInterruptor instance)
        {
            if (instance == null)
            {
                return "<null>";
            }

            return $"HashCode: {instance.GetHashCode()}";
        }
    }

    /// <summary>
    /// when using <see cref="IScopeInterruptor"/>, this class is designed to resolve a service after a scope interruption.
    /// Normally this is not needed, but there are exceptions.
    /// </summary>
    /// <remarks>Note that every T must be registered manually in the container</remarks>
    /// <typeparam name="T"></typeparam>
    public class LateResolver<T>
    {
        private readonly Func<T> factoryMethod;

        public LateResolver(Func<T> factoryMethod)
        {
            this.factoryMethod = factoryMethod;
        }
        public T Resolve()
        {
            return factoryMethod.Invoke();
        }
    }
}