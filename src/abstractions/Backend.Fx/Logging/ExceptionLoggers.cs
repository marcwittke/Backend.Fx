using System;
using System.Collections;
using System.Collections.Generic;

namespace Backend.Fx.Logging
{
    public class ExceptionLoggers : ICollection<IExceptionLogger>, IExceptionLogger
    {
        private static readonly ILogger Logger = LogManager.Create<ExceptionLoggers>();
        private readonly ICollection<IExceptionLogger> _collectionImplementation = new List<IExceptionLogger>();

        public void LogException(Exception ex)
        {
            foreach (IExceptionLogger exceptionLogger in _collectionImplementation)
            {
                try
                {
                    exceptionLogger.LogException(ex);
                }
                catch (Exception ex2)
                {
                    Logger.Error(ex, $"{exceptionLogger.GetType().Name} failed to log the {ex2.GetType()} mith message {ex.Message}");
                }
            }
        }

        public IEnumerator<IExceptionLogger> GetEnumerator()
        {
            return _collectionImplementation.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _collectionImplementation).GetEnumerator();
        }

        public void Add(IExceptionLogger item)
        {
            _collectionImplementation.Add(item);
        }

        public void Clear()
        {
            _collectionImplementation.Clear();
        }

        public bool Contains(IExceptionLogger item)
        {
            return _collectionImplementation.Contains(item);
        }

        public void CopyTo(IExceptionLogger[] array, int arrayIndex)
        {
            _collectionImplementation.CopyTo(array, arrayIndex);
        }

        public bool Remove(IExceptionLogger item)
        {
            return _collectionImplementation.Remove(item);
        }

        public int Count => _collectionImplementation.Count;

        public bool IsReadOnly => _collectionImplementation.IsReadOnly;
    }
}