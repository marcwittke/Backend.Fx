using System.Collections;
using System.Collections.Generic;

namespace Backend.Fx.RandomData
{
    public abstract class Generator<T> : IEnumerable<T>
    {
        public abstract IEnumerator<T> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}