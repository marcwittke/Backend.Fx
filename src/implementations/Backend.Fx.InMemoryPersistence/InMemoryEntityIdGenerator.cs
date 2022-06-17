using Backend.Fx.Patterns.IdGeneration;
using JetBrains.Annotations;

namespace Backend.Fx.InMemoryPersistence
{
    [PublicAPI]
    public class InMemoryEntityIdGenerator : SequenceHiLoIdGenerator, IEntityIdGenerator
    {
        public InMemoryEntityIdGenerator() : base(new InMemorySequence())
        {
        }
    }
}