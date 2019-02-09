using Backend.Fx.BuildingBlocks;
using Backend.Fx.Patterns.DataGeneration;

namespace Backend.Fx.SimpleInjectorDependencyInjection.Tests.DummyImpl.Domain
{
    public class ADemoAggregateGenerator : DataGenerator, IDemoDataGenerator
    {
        private static int _id = 457567;
        public static string Name = "Demo record";

        private readonly IRepository<AnAggregate> _repository;
        public override int Priority => 1;

        public ADemoAggregateGenerator(IRepository<AnAggregate> repository)
        {
            _repository = repository;
        }

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