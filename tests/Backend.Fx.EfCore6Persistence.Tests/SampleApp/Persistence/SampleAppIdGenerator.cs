using Backend.Fx.InMemoryPersistence;
using Backend.Fx.Patterns.IdGeneration;

namespace Backend.Fx.EfCore6Persistence.Tests.SampleApp.Persistence
{
    public class SampleAppIdGenerator : SequenceHiLoIdGenerator, IEntityIdGenerator
    {
        public SampleAppIdGenerator() : base(new InMemorySequence())
        { }
    }
}