namespace Backend.Fx.EfCorePersistence.Tests.DummyImpl
{
    using BuildingBlocks;
    using JetBrains.Annotations;

    public class Post : Entity
    {
        [UsedImplicitly]
        private Post() { }

        public Post(Blog blog, string name)
        {
            Blog = blog;
            BlogId = blog.Id;
            Name = name;
        }

        public int BlogId { get; private set; }
        public Blog Blog { get; private set; }
        public string Name { get; private set; }

        public void SetName(string name)
        {
            Name = name;
        }

        protected override AggregateRoot FindMyAggregateRoot()
        {
            return Blog;
        }
    }
}