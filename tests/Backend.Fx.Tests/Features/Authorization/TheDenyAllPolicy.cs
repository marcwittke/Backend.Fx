using System.Linq;
using Backend.Fx.Domain;
using Backend.Fx.Features.Authorization;
using Xunit;

namespace Backend.Fx.Tests.Features.Authorization;

public class TheDenyAllPolicy
{
    private class Agg : IAggregateRoot<int>
    {
        public Agg(int id)
        {
            Id = id;
        }

        public int Id { get; }
    }
    
    private class DenyAllSut : DenyAll<Agg> {}

    [Fact]
    public void AllowsRead()
    {
        var sut = new DenyAllSut();

        var aggregates = Enumerable.Range(1, 10).Select(i => new Agg(i)).ToArray();

        var count = aggregates.Count(agg => sut.HasAccessExpression.Compile().Invoke(agg));
        Assert.Equal(0, count);
    }

    [Fact]
    public void DeniesCreate()
    {
        var sut = new DenyAllSut();
        Assert.False(sut.CanCreate(new Agg(1)));
    }
    
    [Fact]
    public void DeniesModify()
    {
        var sut = new DenyAllSut();
        Assert.False(sut.CanModify(new Agg(1)));
    }
    
    [Fact]
    public void DeniesDelete()
    {
        var sut = new DenyAllSut();
        Assert.False(sut.CanDelete(new Agg(1)));
    }
}