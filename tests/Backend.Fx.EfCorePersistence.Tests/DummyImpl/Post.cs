namespace Backend.Fx.EfCorePersistence.Tests.DummyImpl
{
    using System.Collections.Generic;
    using BuildingBlocks;

    public class Post : AggregateRoot
    {
        private Post() { }

        public Post(Blog blog, string title, string content)
        {
            BlogId = blog.Id;
            Title = title;
            Content = content;
        }

        public int BlogId { get; private set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public ISet<Comment> Comments { get; } = new HashSet<Comment>();
    }
}