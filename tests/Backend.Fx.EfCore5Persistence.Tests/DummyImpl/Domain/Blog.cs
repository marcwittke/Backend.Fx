using System.Collections.Generic;
using Backend.Fx.BuildingBlocks;
using Backend.Fx.Patterns.IdGeneration;
using JetBrains.Annotations;

namespace Backend.Fx.EfCore5Persistence.Tests.DummyImpl.Domain
{
    public class Blog : AggregateRoot
    {
        [UsedImplicitly]
        private Blog()
        {
        }

        public Blog(int id, string name) : base(id)
        {
            Name = name;
        }

        public string Name { get; private set; }

        public ISet<Post> Posts { get; } = new HashSet<Post>();

        public Post AddPost(IEntityIdGenerator idGenerator, string name, bool isPublic = false)
        {
            var post = new Post(idGenerator.NextId(), this, name, isPublic);
            Posts.Add(post);
            return post;
        }

        public void Modify(string modified)
        {
            Name = modified;
            foreach (Post post in Posts) post.SetName(modified);
        }
    }
}