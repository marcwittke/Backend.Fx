namespace Backend.Fx.EfCorePersistence.Tests.DummyImpl
{
    using BuildingBlocks;

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