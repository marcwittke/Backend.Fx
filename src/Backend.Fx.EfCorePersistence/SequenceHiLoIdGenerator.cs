namespace Backend.Fx.EfCorePersistence
{
    using Patterns.IdGeneration;

    public abstract class SequenceHiLoIdGenerator : HiLoIdGenerator
    {
        public abstract void EnsureSqlSequenceExistence();
    }
}
