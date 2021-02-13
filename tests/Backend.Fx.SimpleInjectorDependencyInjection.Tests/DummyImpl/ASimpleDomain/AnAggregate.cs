using Backend.Fx.BuildingBlocks;

namespace Backend.Fx.SimpleInjectorDependencyInjection.Tests.DummyImpl.ASimpleDomain
{
    public class AnAggregate : AggregateRoot
    {
        public AnAggregate(int id, string name) : base(id)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}
