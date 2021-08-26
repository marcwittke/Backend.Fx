using Backend.Fx.Environment.Persistence;
using Backend.Fx.Patterns.IdGeneration;

namespace Backend.Fx.EfCorePersistence.Tests.Fixtures.MsSql
{
    public class EntityIdGenerator : SequenceHiLoIdGenerator, IEntityIdGenerator
    {
        public EntityIdGenerator(IDbConnectionFactory dbConnectionFactory)
            : base(new EntityIdSequence(dbConnectionFactory))
        { }

        protected override int BlockSize => 1000;
    }
}