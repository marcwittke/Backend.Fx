namespace Backend.Fx.Patterns.DependencyInjection
{
    using System;

    public interface IRuntimeScope : IDisposable, IScopeInterruptor
    {
        void BeginUnitOfWork(bool beginAsReadonlyUnitOfWork);
        void CompleteUnitOfWork();
        TService GetInstance<TService>() where TService : class;
    }
}