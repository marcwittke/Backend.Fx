namespace DemoBlog.Domain
{
    using Backend.Fx.BuildingBlocks;

    public class Comment : Entity
    {
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