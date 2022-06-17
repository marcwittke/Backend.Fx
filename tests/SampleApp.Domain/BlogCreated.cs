using Backend.Fx.Patterns.EventAggregation.Domain;

namespace SampleApp.Domain
{
    public class BlogCreated : IDomainEvent
    {
        public int BlogId { get; }
    
        public BlogCreated(int blogId)
        {
            BlogId = blogId;
        }
    }
}