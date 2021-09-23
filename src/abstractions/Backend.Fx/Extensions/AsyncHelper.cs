using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Backend.Fx.Extensions
{
    public static class AsyncHelper
    {
        /// <summary>
        /// Executes an async Task method which has a void return value synchronously
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
                var syncContext = new ExclusiveSynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(syncContext);
                syncContext.Post(
                    // ReSharper disable once AsyncVoidLambda
                    async _ =>
                    {
                        try
                        {
                            await task();
                        }
                        catch (Exception e)
                        {
                            syncContext.InnerException = e;
                            throw;
                        }
                        finally
                        {
                            syncContext.EndMessageLoop();
                        }
                    },
                    null);
                syncContext.BeginMessageLoop();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(oldContext);
            }
        }

        /// <summary>
        /// Executes an async Task&lt;T&gt; method which has a T return type synchronously
        /// </summary>
        /// <typeparam name="T">Return Type</typeparam>
        /// <param name="task">TaskTask&lt;T&gt; method to execute</param>
        /// <returns></returns>
        public static T RunSync<T>(Func<Task<T>> task)
        {
            var ret = default(T);
            var oldContext = SynchronizationContext.Current;
            try
            {
                var exclusiveSynchronizationContext = new ExclusiveSynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(exclusiveSynchronizationContext);
                exclusiveSynchronizationContext.Post(
                    // ReSharper disable once AsyncVoidLambda
                    async _ =>
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
                    },
                    null);
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
            private readonly Queue<Tuple<SendOrPostCallback, object>> _items
                = new Queue<Tuple<SendOrPostCallback, object>>();

            private readonly AutoResetEvent _workItemsWaiting = new AutoResetEvent(false);
            private bool _done;

            public Exception InnerException { private get; set; }

            public override void Send(SendOrPostCallback d, object state)
            {
                throw new NotSupportedException("We cannot send to our same thread");
            }

            public override void Post(SendOrPostCallback d, object state)
            {
                lock (_items)
                {
                    _items.Enqueue(Tuple.Create(d, state));
                }

                _workItemsWaiting.Set();
            }

            public void EndMessageLoop()
            {
                Post(_ => _done = true, null);
            }

            public void BeginMessageLoop()
            {
                while (!_done)
                {
                    Tuple<SendOrPostCallback, object> task = null;
                    lock (_items)
                    {
                        if (_items.Count > 0)
                        {
                            task = _items.Dequeue();
                        }
                    }

                    if (task != null)
                    {
                        task.Item1(task.Item2);
                        if (InnerException != null) // the method threw an exception
                        {
                            throw new AggregateException("AsyncHelpers.Run method threw an exception.", InnerException);
                        }
                    }
                    else
                    {
                        _workItemsWaiting.WaitOne();
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
