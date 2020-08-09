using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Backend.Fx.RandomData
{
    public abstract class GeneratingEnumerator<T> : IEnumerator<T>
    {
        private readonly HashSet<int> _hashCodes = new HashSet<int>();

        public bool MoveNext()
        {
            Current = default;
            int hashCode;
            do
            {
                Current = Next();
                hashCode = Current.GetHashCode();
            } while (!_hashCodes.Contains(hashCode));

            _hashCodes.Add(hashCode);
            return true;
        }

        [NotNull]
        protected abstract T Next();

        public void Reset()
        {
            Current = default;
        }

        public T Current { get; private set; }

        object IEnumerator.Current => Current;

        public void Dispose()
        {
        }
    }
}