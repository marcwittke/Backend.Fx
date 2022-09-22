using Backend.Fx.Domain;
using Backend.Fx.Extensions.Persistence;
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