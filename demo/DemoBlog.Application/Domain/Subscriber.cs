namespace DemoBlog.Domain
{
    using Backend.Fx.BuildingBlocks;

    public class Subscriber : Entity
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public Blog Blog { get; set; }
        public int BlogId { get; set; }
        protected override AggregateRoot FindMyAggregateRoot()
        {
            return Blog;
        }
    }
}
