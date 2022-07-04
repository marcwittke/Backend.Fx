using Backend.Fx.Features.Persistence;

namespace Backend.Fx.InMemoryPersistence
{
    public class InMemorySequence : ISequence
    {
        private int _currentValue = 1;

        public InMemorySequence(int increment = 1)
        {
            Increment = increment;
        }

        public void EnsureSequence()
        { }

        public int GetNextValue()
        {
            lock (this)
            {
                int nextValue = _currentValue;
                _currentValue += Increment;
                return nextValue;
            }
        }

        public int Increment { get; }
    }
}