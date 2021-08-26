using Backend.Fx.Environment.Persistence;

namespace Backend.Fx.EfCorePersistence.Tests.Fixtures.MsSql
{
    public class EntityIdSequence : MsSqlSequence
    {
        public override int Increment { get; } = 1000;
        protected override string SequenceName { get; } = "SEQ_EntityId";

        public EntityIdSequence(IDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory)
        {
        }
    }
}