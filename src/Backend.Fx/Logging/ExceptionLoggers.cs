namespace Backend.Fx.Logging
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public class ExceptionLoggers : ICollection<IExceptionLogger>, IExceptionLogger
    {
        private static readonly ILogger Logger = LogManager.Create<ExceptionLoggers>();
        private readonly ICollection<IExceptionLogger> collectionImplementation = new List<IExceptionLogger>();

        public void LogException(Exception ex)
        {
            collectionImplementation.AsParallel().ForAll(exceptionLogger =>
                                                         {
                                                             try
                                                             {
                                                                 exceptionLogger.LogException(ex);
                                                             }
                                                             catch (Exception ex2)
                                                             {
                                                                 Logger.Error(ex, $"{exceptionLogger.GetType().Name} failed to log the {ex2.GetType()} mith message {ex.Message}");
                                                             }
                                                         });
        }

        public IEnumerator<IExceptionLogger> GetEnumerator()
        {
            return collectionImplementation.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)collectionImplementation).GetEnumerator();
        }

        public void Add(IExceptionLogger item)
        {
            collectionImplementation.Add(item);
        }

        public void Clear()
        {
            collectionImplementation.Clear();
        }

        public bool Contains(IExceptionLogger item)
        {
            return collectionImplementation.Contains(item);
        }

        public void CopyTo(IExceptionLogger[] array, int arrayIndex)
        {
            collectionImplementation.CopyTo(array, arrayIndex);
        }

        public bool Remove(IExceptionLogger item)
        {
            return collectionImplementation.Remove(item);
        }

        public int Count
        {
            get { return collectionImplementation.Count; }
        }

        public bool IsReadOnly
        {
            get { return collectionImplementation.IsReadOnly; }
        }
    }
}