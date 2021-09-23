namespace Backend.Fx.Exceptions
{
    public class NotFoundException : ClientException
    {
        public NotFoundException()
            : base("Not found.")
        { }

        public NotFoundException(string entityName, object id)
            : base($"No {entityName}[{id}] found.")
        {
            EntityName = entityName;
            Id = id;
        }

        public string EntityName { get; }

        public object Id { get; }
    }


    public class NotFoundException<TEntity> : NotFoundException
    {
        public NotFoundException(object id)
            : base(typeof(TEntity).Name, id)
        { }
    }
}
