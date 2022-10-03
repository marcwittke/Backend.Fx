namespace Backend.Fx.Features.IdGeneration
{
    public interface IEntityIdGenerator<out TId> : IIdGenerator<TId> where TId : struct
    {
    }
    
    public class EntityIdGenerator<TId> : IEntityIdGenerator<TId> where TId : struct
    {
        private readonly IIdGenerator<TId> _idGenerator;

        public EntityIdGenerator(IIdGenerator<TId> idGenerator)
        {
            _idGenerator = idGenerator;
        }

        public TId NextId()
        {
            return _idGenerator.NextId();
        }
    }
}
