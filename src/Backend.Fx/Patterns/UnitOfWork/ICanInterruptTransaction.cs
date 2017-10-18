namespace Backend.Fx.Patterns.UnitOfWork
{
    using System;

    /// <summary>
    /// Require this interface in your constructor, if you need an action or function to run outside a transaction
    /// Normally this is not needed, but there are exceptions: Sending an SMTP Message for instance might
    /// take a few seconds, or even worse, you cannot predict the time consumption, since it depends on
    /// an external service. It is wise to commit a running transaction before starting the SMTP communication
    /// and start a new one when it's done to minimize the probability of deadlocks.
    /// 
    /// Nevertheless, this requires careful design of the domain logic. An SMTP message for example can't be 
    /// just "pending" or "done". During the period without transaction, the specific email must be persisted
    /// as "processing" to be able to recompense after failures.
    /// </summary>
    public interface ICanInterruptTransaction
    {
        /// <summary>
        /// Completes the current transaction, runs the action and begins a new transaction
        /// </summary>
        void CompleteCurrentTransaction_InvokeAction_BeginNewTransaction(Action action);

        /// <summary>
        /// Completes the current transaction, runs the function and begins a new transaction
        /// </summary>
        T CompleteCurrentTransaction_InvokeFunction_BeginNewTransaction<T>(Func<T> func);

        /// <summary>
        /// Completes the current and begins a new transaction
        /// </summary>
        void CompleteCurrentTransaction_BeginNewTransaction();
    }    
}