namespace Backend.Fx.Features.IdGeneration
{
    public interface IEntityIdGenerator : IIdGenerator
    {
    }
    
    public class EntityIdGenerator : IEntityIdGenerator
    {
        private readonly IIdGenerator _idGenerator;

        public EntityIdGenerator(IIdGenerator idGenerator)
        {
            _idGenerator = idGenerator;
        }

        public int NextId()
        {
            return _idGenerator.NextId();
        }
    }
}
