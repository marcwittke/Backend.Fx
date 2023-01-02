using System.Linq;
using Backend.Fx.Domain;
using Backend.Fx.Features.Authorization;
using Xunit;

namespace Backend.Fx.Tests.Features.Authorization;

public class TheAllowAllPolicy
{
    private class Agg : IAggregateRoot<int>
    {
        public Agg(int id)
        {
            Id = id;
        }

        public int Id { get; }
    }
    
    private class AllowAllSut : AllowAll<Agg> {}

    [Fact]
    public void AllowsRead()
    {
        var sut = new AllowAllSut();

        var aggregates = Enumerable.Range(1, 10).Select(i => new Agg(i)).ToArray();

        var count = aggregates.Count(agg => sut.HasAccessExpression.Compile().Invoke(agg));
        Assert.Equal(10, count);
    }

    [Fact]
    public void AllowsCreate()
    {
        var sut = new AllowAllSut();
        Assert.True(sut.CanCreate(new Agg(1)));
    }
    
    [Fact]
    public void AllowsModify()
    {
        var sut = new AllowAllSut();
        Assert.True(sut.CanModify(new Agg(1)));
    }
    
    [Fact]
    public void AllowsDelete()
    {
        var sut = new AllowAllSut();
        Assert.True(sut.CanDelete(new Agg(1)));
    }
}