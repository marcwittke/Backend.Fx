namespace DemoBlog.Domain
{
    using Backend.Fx.BuildingBlocks;
    using JetBrains.Annotations;

    public class Subscriber : Entity
    {
        [UsedImplicitly]
        public Subscriber() { }

        public Subscriber(int id, string name, string email, Blog blog)
        {
            Id = id;
            Name = name;
            Email = email;
            Blog = blog;
            BlogId = blog.Id;
        }

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
