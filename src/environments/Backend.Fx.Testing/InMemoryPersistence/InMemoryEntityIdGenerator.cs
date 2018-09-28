namespace Backend.Fx.Testing.InMemoryPersistence
{
    using Patterns.IdGeneration;

    public class InMemoryEntityIdGenerator : IEntityIdGenerator
    {
        private int _nextId = 1;

        public int NextId()
        {
            return _nextId++;
        }
    }
}