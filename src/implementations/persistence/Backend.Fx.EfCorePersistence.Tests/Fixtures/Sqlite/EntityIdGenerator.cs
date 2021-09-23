using Backend.Fx.Patterns.IdGeneration;

namespace Backend.Fx.EfCorePersistence.Tests.Fixtures.Sqlite
{
    public class EntityIdGenerator : SequenceHiLoIdGenerator, IEntityIdGenerator
    {
        public EntityIdGenerator()
            : base(new FakeSequence())
        { }

        protected override int BlockSize => 1000;
    }
}
