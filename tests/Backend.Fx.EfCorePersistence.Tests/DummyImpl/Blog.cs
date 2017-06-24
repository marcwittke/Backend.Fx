namespace Backend.Fx.EfCorePersistence.Tests.DummyImpl
{
    using System.Collections.Generic;
    using BuildingBlocks;
    using JetBrains.Annotations;
    using Patterns.IdGeneration;

    public class Blog : AggregateRoot
    {
        [UsedImplicitly]
        private Blog() { }

        public Blog(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public string Name { get; private set; }

        public ISet<Post> Posts { get; } = new HashSet<Post>();

        public Post AddPost(IEntityIdGenerator idGenerator, string name)
        {
            var post = new Post(idGenerator.NextId(), this, name);
            Posts.Add(post);
            return post;
        }

        public void Modify(string modified)
        {
            Name = modified;
            foreach (var post in Posts)
            {
                post.SetName(modified);
            }
        }
    }
}