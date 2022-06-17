using Backend.Fx.BuildingBlocks;
using Backend.Fx.Patterns.EventAggregation.Domain;
using Xunit;

namespace SampleApp.Domain
{
    public class BlogCreatedHandler : IDomainEventHandler<BlogCreated>
    {
        private readonly IRepository<Blog> _blogRepository;

        public static int InvocationCount = 0;
        public static int ExpectedInvocationCount = 0;
    
        public BlogCreatedHandler(IRepository<Blog> blogRepository)
        {
            _blogRepository = blogRepository;
        }
    
        public void Handle(BlogCreated domainEvent)
        {
            InvocationCount++;
            Assert.NotNull(_blogRepository.SingleOrDefault(domainEvent.BlogId));
        }
    }
}