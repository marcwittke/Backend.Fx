using JetBrains.Annotations;

namespace Backend.Fx.Exceptions
{
    [PublicAPI]
    public class NotFoundException : ClientException
    {
        public string EntityName { get; }

        public object Id { get; }

        public NotFoundException()
            : base("Not found.")
        {
        }

        public NotFoundException(string entityName, object id)
            : base($"No {entityName}[{id}] found.")
        {
            EntityName = entityName;
            Id = id;
        }

        /// <summary>
        /// Used to build a <see cref="NotFoundException"/> with multiple possible error messages. The builder will throw on disposal
        /// when at least one error was added. Using the AddIf methods is quite comfortable when there are several criteria to be validated
        /// before executing a business case. 
        /// </summary>
        public static IExceptionBuilder UseBuilder()
        {
            return new ExceptionBuilder<NotFoundException>();
        }
    }

    public class NotFoundException<TEntity> : NotFoundException
    {
        public NotFoundException(object id)
            : base(typeof(TEntity).Name, id)
        {
        }
    }
}