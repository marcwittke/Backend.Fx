namespace Backend.Fx.Exceptions
{
    public class NotFoundException<TAggregateRoot> : NotFoundException
    {
        public NotFoundException(int id) : base(typeof(TAggregateRoot).Name, id)
        {}
    }

    public abstract class NotFoundException : ClientException
    {

        public string AggregateName { get; }

        public int Id { get; }

        protected NotFoundException(string aggregateName, int id) : base($"No {aggregateName} with id {id} found.")
        {
            AggregateName = aggregateName;
            Id = id;
        }
    }
}