using Backend.Fx.Domain;

namespace Backend.Fx.Tests.DummyServices;

public class DummyAggregate : IAggregateRoot<int>
{

    public DummyAggregate(int id, string name)
    {
        Id = id;
        Name = name;
    }
    
    public int Id { get; }

    public string Name { get; }
}