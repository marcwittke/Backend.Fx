namespace Backend.Fx.Features.IdGeneration.InMem
{
    public class InMemorySequence : ISequence<int>
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
                var nextValue = _currentValue;
                _currentValue += Increment;
                return nextValue;
            }
        }

        public int Increment { get; }
    }
}