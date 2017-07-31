namespace DemoBlog.Domain
{
    using Backend.Fx.BuildingBlocks;
    using JetBrains.Annotations;

    public class Comment : Entity
    {
        [UsedImplicitly]
        private Comment()
        {}

        public Comment(int id, Post post, string author, string content) : base(id)
        {
            Post = post;
            PostId = post.Id;
            Author = author;
            Content = content;
        }

        public int PostId { get; set; }
        public Post Post { get; private set; }

        public int InReplyToCommentId { get; set; }
        
        public string Author { get; set; }

        public string Content { get; set; }

        protected override AggregateRoot FindMyAggregateRoot()
        {
            return Post;
        }
    }
}