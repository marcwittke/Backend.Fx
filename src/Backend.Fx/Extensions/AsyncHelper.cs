﻿namespace Backend.Fx.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public static class AsyncHelper
    {
        /// <summary>
        /// Execute's an async Task method which has a void return value synchronously
        /// </summary>
        /// <param name="task">Task method to execute</param>
        /// <remarks>
        /// See https://stackoverflow.com/questions/5095183/how-would-i-run-an-async-taskt-method-synchronously
        /// This code was the only possibility to make a task synchronously without deadlocking on the SingleCPU Build agent
        /// </remarks>
        public static void RunSync(Func<Task> task)
        {
            var oldContext = SynchronizationContext.Current;
            try
            {
                var synch = new ExclusiveSynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(synch);
                synch.Post(async _ =>
                           {
                               try
                               {
                                   await task();
                               }
                               catch (Exception e)
                               {
                                   synch.InnerException = e;
                                   throw;
                               }
                               finally
                               {
                                   synch.EndMessageLoop();
                               }
                           }, null);
                synch.BeginMessageLoop();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(oldContext);
            }
        }

        /// <summary>
        /// Execute's an async Task&lt;T&gt; method which has a T return type synchronously
        /// </summary>
        /// <typeparam name="T">Return Type</typeparam>
        /// <param name="task">TaskTask&lt;T&gt; method to execute</param>
        /// <returns></returns>
        public static T RunSync<T>(Func<Task<T>> task)
        {
            T ret = default(T);
            var oldContext = SynchronizationContext.Current;
            try
            {
                var exclusiveSynchronizationContext = new ExclusiveSynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(exclusiveSynchronizationContext);
                exclusiveSynchronizationContext.Post(async _ =>
                           {
                               try
                               {
                                   ret = await task();
                               }
                               catch (Exception e)
                               {
                                   exclusiveSynchronizationContext.InnerException = e;
                                   throw;
                               }
                               finally
                               {
                                   exclusiveSynchronizationContext.EndMessageLoop();
                               }
                           }, null);
                exclusiveSynchronizationContext.BeginMessageLoop();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(oldContext);
            }
            return ret;
        }

        private class ExclusiveSynchronizationContext : SynchronizationContext
        {
            private bool done;
            public Exception InnerException { get; set; }
            readonly AutoResetEvent workItemsWaiting = new AutoResetEvent(false);
            readonly Queue<Tuple<SendOrPostCallback, object>> items = new Queue<Tuple<SendOrPostCallback, object>>();

            public override void Send(SendOrPostCallback d, object state)
            {
                throw new NotSupportedException("We cannot send to our same thread");
            }

            public override void Post(SendOrPostCallback d, object state)
            {
                lock (items)
                {
                    items.Enqueue(Tuple.Create(d, state));
                }
                workItemsWaiting.Set();
            }

            public void EndMessageLoop()
            {
                Post(_ => done = true, null);
            }

            public void BeginMessageLoop()
            {
                while (!done)
                {
                    Tuple<SendOrPostCallback, object> task = null;
                    lock (items)
                    {
                        if (items.Count > 0)
                        {
                            task = items.Dequeue();
                        }
                    }
                    if (task != null)
                    {
                        task.Item1(task.Item2);
                        if (InnerException != null) // the method threw an exeption
                        {
                            throw new AggregateException("AsyncHelpers.Run method threw an exception.", InnerException);
                        }
                    }
                    else
                    {
                        workItemsWaiting.WaitOne();
                    }
                }
            }

            public override SynchronizationContext CreateCopy()
            {
                return this;
            }
        }
    }
}