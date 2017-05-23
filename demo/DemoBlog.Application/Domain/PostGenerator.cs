namespace DemoBlog.Domain
{
    using Backend.Fx.BuildingBlocks;
    using Backend.Fx.Patterns.DataGeneration;
    using Backend.Fx.RandomData;

    public class PostGenerator : InitialDataGenerator, IDemoDataGenerator
    {
        private readonly IRepository<Blog> blogRepository;
        private readonly IRepository<Post> postRepository;

        public PostGenerator(IRepository<Blog> blogRepository, IRepository<Post> postRepository)
        {
            this.blogRepository = blogRepository;
            this.postRepository = postRepository;
        }
        public override int Priority
        {
            get { return 30; }
        }

        protected override void GenerateCore()
        {
            foreach (var blog in blogRepository.GetAll())
            {
                for (int i = 0; i < TestRandom.Next(10); i++)
                {
                    postRepository.Add(new Post(blog, LoremIpsum.Generate(5, 10, false), LoremIpsum.Generate(200, 500, false)));
                }
            }
        }

        protected override void Initialize()
        {}

        protected override bool ShouldRun()
        {
            return !postRepository.Any();
        }
    }
}