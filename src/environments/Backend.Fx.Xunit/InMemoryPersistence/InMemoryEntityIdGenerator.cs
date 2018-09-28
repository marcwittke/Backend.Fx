using Backend.Fx.Patterns.IdGeneration;

namespace Backend.Fx.Xunit.InMemoryPersistence
{
    public class InMemoryEntityIdGenerator : IEntityIdGenerator
    {
        private int _nextId = 1;

        public int NextId()
        {
            return _nextId++;
        }
    }
}