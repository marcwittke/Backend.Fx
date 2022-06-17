using Backend.Fx.InMemoryPersistence;
using Backend.Fx.Patterns.IdGeneration;

namespace Backend.Fx.EfCore5Persistence.Tests.SampleApp.Persistence
{
    public class SampleAppIdGenerator : SequenceHiLoIdGenerator, IEntityIdGenerator
    {
        public SampleAppIdGenerator() : base(new InMemorySequence())
        { }
    }
}