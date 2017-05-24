namespace Backend.Fx.Tests.Patterns.UnitOfWork
{
    using Fx.Patterns.UnitOfWork;

    public class TestReadonlyUnitOfWork : ReadonlyUnitOfWork
    {
        public int RollbackCount { get; private set; }

        protected override void Rollback()
        {
            RollbackCount++;
        }
    }
}