namespace DemoBlog.Domain
{
    using Backend.Fx.BuildingBlocks;
    using Backend.Fx.Patterns.DataGeneration;
    using Backend.Fx.Patterns.IdGeneration;
    using Backend.Fx.RandomData;

    public class BlogGenerator : InitialDataGenerator, IDemoDataGenerator
    {
        private readonly IEntityIdGenerator idGenerator;
        private readonly IRepository<Blogger> bloggerRepository;
        private readonly IRepository<Blog> blogRepository;

        public BlogGenerator(IEntityIdGenerator idGenerator, IRepository<Blogger> bloggerRepository, IRepository<Blog> blogRepository)
        {
            this.idGenerator = idGenerator;
            this.bloggerRepository = bloggerRepository;
            this.blogRepository = blogRepository;
        }

        public override int Priority
        {
            get { return 20; }
        }

        protected override void GenerateCore()
        {
            for (int i = 0; i < 10; i++)
            {
                string title = LoremIpsum.Generate(3, 8, false);
                string description = LoremIpsum.Generate(40, 60, true);
                blogRepository.Add(new Blog(idGenerator.NextId(), bloggerRepository.AggregateQueryable.Random(), title, description));
            }
        }

        protected override void Initialize()
        {}

        protected override bool ShouldRun()
        {
            return !blogRepository.Any();
        }
    }
}
