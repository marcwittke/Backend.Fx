using Backend.Fx.BuildingBlocks;
using Backend.Fx.Patterns.DataGeneration;

namespace Backend.Fx.SimpleInjectorDependencyInjection.Tests.DummyImpl.ASimpleDomain
{
    public class ADemoAggregateGenerator : DataGenerator, IDemoDataGenerator
    {
        private static int _id = 457567;
        public static string Name = "Demo record";

        private readonly IRepository<AnAggregate> _repository;

        public ADemoAggregateGenerator(IRepository<AnAggregate> repository)
        {
            _repository = repository;
        }

        public override int Priority => 1;

        protected override void GenerateCore()
        {
            _repository.Add(new AnAggregate(_id++, Name));
        }

        protected override void Initialize()
        { }

        protected override bool ShouldRun()
        {
            return true;
        }
    }
}
