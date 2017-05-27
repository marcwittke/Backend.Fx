namespace Backend.Fx.EfCorePersistence.Tests.DummyImpl
{
    using System.Collections.Generic;
    using BuildingBlocks;
    using JetBrains.Annotations;

    public class Blog : AggregateRoot
    {
        [UsedImplicitly]
        private Blog() { }

        public Blog(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }

        public ISet<Post> Posts { get; private set; } = new HashSet<Post>();

        public Post AddPost(string name)
        {
            var post = new Post(this, name);
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