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

        public object Id { get; }

        protected NotFoundException(string entityName, object id) : base($"No {entityName}[{id}] found.")
        {
            EntityName = entityName;
            Id = id;
        }
    }
}