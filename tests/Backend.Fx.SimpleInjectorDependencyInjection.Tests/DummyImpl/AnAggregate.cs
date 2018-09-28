using Backend.Fx.BuildingBlocks;
using Backend.Fx.Patterns.Authorization;
using Backend.Fx.Patterns.DataGeneration;

namespace Backend.Fx.SimpleInjectorDependencyInjection.Tests.DummyImpl
{
    public class AnAggregate : AggregateRoot
    {
        public AnAggregate(int id, string name) : base(id)
        {
            Name = name;
        }

        public string Name { get; set; }
    }

    public class ProductiveGenerator : InitialDataGenerator, IProductiveDataGenerator
    {
        private readonly IRepository<AnAggregate> _repository;
        public override int Priority => 1;

        public ProductiveGenerator(IRepository<AnAggregate> repository)
        {
            _repository = repository;
        }

        protected override void GenerateCore()
        {
            _repository.Add(new AnAggregate(234, "Productive record"));
        }

        protected override void Initialize()
        { }

        protected override bool ShouldRun()
        {
            return true;
        }
    }

    public class AnAggregateAuthorization : AllowAll<AnAggregate> { }

    public class DemonstrationGenerator : InitialDataGenerator, IDemoDataGenerator
    {
        private readonly IRepository<AnAggregate> _repository;
        public override int Priority => 1;

        public DemonstrationGenerator(IRepository<AnAggregate> repository)
        {
            _repository = repository;
        }

        protected override void GenerateCore()
        {
            _repository.Add(new AnAggregate(456, "Demo record"));
        }

        protected override void Initialize()
        { }

        protected override bool ShouldRun()
        {
            return true;
        }
    }
}
