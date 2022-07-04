using Backend.Fx.Features.Persistence;
using Backend.Fx.InMemoryPersistence;

namespace Backend.Fx.EfCore6Persistence.Tests.SampleApp.Persistence
{
    public class SampleAppIdGenerator : SequenceHiLoIdGenerator, IEntityIdGenerator
    {
        public SampleAppIdGenerator() : base(new InMemorySequence())
        { }
    }
}