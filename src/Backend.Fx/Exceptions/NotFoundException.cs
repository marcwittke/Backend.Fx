namespace Backend.Fx.Exceptions
{
    public class NotFoundException<TEntity> : NotFoundException
    {
        public NotFoundException(int id) : base(typeof(TEntity).Name, id)
        {}
    }

    public abstract class NotFoundException : ClientException
    {

        public string EntityName { get; }

        public int Id { get; }

        protected NotFoundException(string entityName, int id) : base($"No {entityName} with id {id} found.")
        {
            EntityName = entityName;
            Id = id;
        }
    }
}