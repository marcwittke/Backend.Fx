using Backend.Fx.Patterns.IdGeneration;

namespace Backend.Fx.InMemoryPersistence
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
