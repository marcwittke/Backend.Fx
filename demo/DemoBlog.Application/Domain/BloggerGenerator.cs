namespace DemoBlog.Domain
{
    using Backend.Fx.BuildingBlocks;
    using Backend.Fx.Patterns.DataGeneration;
    using Backend.Fx.RandomData;

    public class BloggerGenerator : InitialDataGenerator, IDemoDataGenerator
    {
        private readonly IRepository<Blogger> bloggerRepository;

        public BloggerGenerator(IRepository<Blogger> bloggerRepository)
        {
            this.bloggerRepository = bloggerRepository;
        }

        public override int Priority
        {
            get { return 10; }
        }

        protected override void GenerateCore()
        {
            foreach (var testPerson in new TestPersonGenerator { GenerateCount = 10 }.GenerateTestPersons())
            {
                bloggerRepository.Add(new Blogger(testPerson.LastName, testPerson.FirstName));
            }
        }

        protected override void Initialize()
        {}

        protected override bool ShouldRun()
        {
            return !bloggerRepository.Any();
        }
    }
}