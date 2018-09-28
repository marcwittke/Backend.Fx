namespace Backend.Fx.Exceptions
{
    public class NotFoundException<TEntity> : NotFoundException
    {
        public NotFoundException(object id, params Error[] errors) 
                : base(typeof(TEntity).Name, id, errors)
        {}
    }

    public class NotFoundException : ClientException
    {
        public string EntityName { get; }

        public object Id { get; }

        public NotFoundException()
                : base("Not found.") 
        {}

        public NotFoundException(params Error[] errors) 
                : base("Not found.", errors) 
        {}

        public NotFoundException(string entityName, object id, params Error[] errors)
                : base($"No {entityName}[{id}] found.", errors)
        {
            EntityName = entityName;
            Id = id;
        }

        public static IExceptionBuilder UseBuilder()
        {
            return new ExceptionBuilder<NotFoundException>();
        }
    }
}