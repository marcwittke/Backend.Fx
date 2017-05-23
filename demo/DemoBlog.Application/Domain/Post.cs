namespace DemoBlog.Domain
{
    using System.Collections.Generic;
    using Backend.Fx.BuildingBlocks;

    public class Post : AggregateRoot
    {
        private Post() {}

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