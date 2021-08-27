using Backend.Fx.Patterns.IdGeneration;

namespace Backend.Fx.EfCorePersistence.Tests.Fixtures
{
    public class FakeSequence : ISequence
    {
        private static int _currentValue = 1;
        
        public void EnsureSequence()
        {
        }

        public int GetNextValue()
        {
            _currentValue += Increment;
            return _currentValue;
        }

        public int Increment => 1000;
    }
}