using Backend.Fx.Domain;
using Backend.Fx.Extensions.Persistence;
using Backend.Fx.InMemoryPersistence;

namespace Backend.Fx.EfCore5Persistence.Tests.SampleApp.Persistence
{
    public class SampleAppIdGenerator : SequenceHiLoIdGenerator, IEntityIdGenerator
    {
        public SampleAppIdGenerator() : base(new InMemorySequence())
        { }
    }
}