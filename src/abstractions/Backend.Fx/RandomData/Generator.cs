using System.Collections;
using System.Collections.Generic;

namespace Backend.Fx.RandomData
{
    public abstract class Generator<T> : IEnumerable<T>
    {
        private readonly HashSet<int> _identicalPreventionMemory = new HashSet<int>();

        public IEnumerator<T> GetEnumerator()
        {
            while (true)
            {
                T next;
                int hashCode;
                do
                {
                    next = Next();
                    hashCode = next.GetHashCode();
                } while (_identicalPreventionMemory.Contains(hashCode));

                _identicalPreventionMemory.Add(hashCode);
                yield return next;
            }
            // ReSharper disable once IteratorNeverReturns
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        protected abstract T Next();
    }
}