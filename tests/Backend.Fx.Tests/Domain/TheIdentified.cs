using Backend.Fx.Domain;
using Xunit;

namespace Backend.Fx.Tests.Domain;

public class TheIdentified
{
    [Fact]
    public void HasDebuggerDisplay()
    {
        var someEntity = new SomeEntity(new EntityId(1), "Entity 1");
        Assert.Equal("SomeEntity[1]", someEntity.DebuggerDisplay);
    }

    [Fact]
    public void ConsidersSameObjectToBeEqual()
    {
        var someEntity = new SomeEntity(new EntityId(1), "Entity 1");

        Assert.True(someEntity.Equals(someEntity));
        Assert.True(someEntity == someEntity);
        Assert.False(someEntity != someEntity);
        Assert.True(Equals(someEntity, someEntity));
        Assert.StrictEqual(someEntity, someEntity);
        Assert.Equal(someEntity, someEntity);
    }
    
    [Fact]
    public void ConsidersDifferentObjectsWithSameIdToBeEqual()
    {
        var someEntity = new SomeEntity(new EntityId(1), "Entity 1");
        var someEntityToo = new SomeEntity(new EntityId(1), "Entity 1");

        Assert.True(someEntity.Equals(someEntityToo));
        Assert.True(someEntity == someEntityToo);
        Assert.False(someEntity != someEntityToo);
        Assert.True(Equals(someEntity, someEntityToo));
        Assert.StrictEqual(someEntity, someEntityToo);
        Assert.Equal(someEntity, someEntityToo);
    }
    
    [Fact]
    public void ConsidersDifferentInstancesNotToBeEqual()
    {
        var someEntity1 = new SomeEntity(new EntityId(1), "Entity 1");
        var someEntity2 = new SomeEntity(new EntityId(2), "Entity 2");

        Assert.False(someEntity1.Equals(someEntity2));
        Assert.False(someEntity1 == someEntity2);
        Assert.True(someEntity1 != someEntity2);
        Assert.False(Equals(someEntity1, someEntity2));
        Assert.NotStrictEqual(someEntity1, someEntity2);
        Assert.NotEqual(someEntity1, someEntity2);
    }
    
    [Fact]
    public void ConsidersDifferentObjectsNotToBeEqual()
    {
        var someEntity1 = new SomeEntity(new EntityId(1), "Entity 1");
        var somethingCompletelyDifferent = new object();

        Assert.False(someEntity1.Equals(somethingCompletelyDifferent));
        Assert.False(someEntity1 == somethingCompletelyDifferent);
        Assert.True(someEntity1 != somethingCompletelyDifferent);
        Assert.False(Equals(someEntity1, somethingCompletelyDifferent));
        Assert.NotStrictEqual(someEntity1, somethingCompletelyDifferent);
        Assert.NotEqual(someEntity1, somethingCompletelyDifferent);
    }

    [Fact]
    public void EqualInstancesHaveEqualHashCodes()
    {
        var someEntity1 = new SomeEntity(new EntityId(1), "Entity 1");
        var someEntity2 = new SomeEntity(new EntityId(1), "Entity 1");
        Assert.Equal(someEntity1.GetHashCode(), someEntity2.GetHashCode());
    }
        
    [Fact]
    public void NotEqualInstancesHaveEqualHashCodes()
    {
        var someEntity1 = new SomeEntity(new EntityId(1), "Entity 1");
        var someEntity2 = new SomeEntity(new EntityId(2), "Entity 2");
        Assert.NotEqual(someEntity1.GetHashCode(), someEntity2.GetHashCode());
    }
    
    private readonly struct EntityId
    {
        public EntityId(int value)
        {
            Value = value;
        }

        public int Value { get; }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
    
    private class SomeEntity : Identified<EntityId>
    {
        public SomeEntity(EntityId id, string name) : base(id)
        {
            Name = name;
        }
        
        public string Name { get; }
    }
}