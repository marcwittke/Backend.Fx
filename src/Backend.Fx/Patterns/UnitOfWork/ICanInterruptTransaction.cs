namespace Backend.Fx.Patterns.UnitOfWork
{
    using System;

    /// <summary>
    /// Require this class in your constructor, if you require an action to run outside a transaction
    /// Normally this is not needed, but there are exceptions.
    /// </summary>
    public interface ICanInterruptTransaction
    {
        void CompleteCurrentTransaction_InvokeAction_BeginNewTransaction(Action action);

        T CompleteCurrentTransaction_InvokeFunction_BeginNewTransaction<T>(Func<T> func);

        void CompleteCurrentTransaction_BeginNewTransaction();
    }    
}