using Backend.Fx.BuildingBlocks;
using JetBrains.Annotations;

namespace Backend.Fx.EfCorePersistence.Tests.DummyImpl.Domain
{
    public class Post : Entity
    {
        [UsedImplicitly]
        private Post() { }

        public Post(int id, Blog blog, string name) : base(id)
        {
            Blog = blog;
            BlogId = blog.Id;
            Name = name;
        }

        public int BlogId { get; [UsedImplicitly] private set; }
        public Blog Blog { get; [UsedImplicitly] private set; }
        public string Name { get; private set; }

        public void SetName(string name)
        {
            Name = name;
        }
    }
}