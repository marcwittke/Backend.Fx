using Backend.Fx.ExecutionPipeline;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.ExecutionPipeline;

public class TheAnonymousIdentity : TestWithLogging
{
    public TheAnonymousIdentity(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void HasCorrectName()
    {
        var sut = new AnonymousIdentity();
        Assert.Equal("ANONYMOUS", sut.Name);
    }
    
    [Fact]
    public void IsNotAuthenticated()
    {
        var sut = new AnonymousIdentity();
        Assert.False(sut.IsAuthenticated);
    }
    
    [Fact]
    public void HasNoAuthenticationType()
    {
        var sut = new AnonymousIdentity();
        Assert.Null(sut.AuthenticationType);
    }
    
    [Fact]
    public void EqualsOtherAnonymousIdentity()
    {
        var sut = new AnonymousIdentity();
        var other = new AnonymousIdentity();
        Assert.True(sut.Equals(other));
        Assert.True(Equals(sut,other));
        Assert.Equal(sut.GetHashCode(), other.GetHashCode());
    }
    
    [Fact]
    public void DoesNotEqualOtherIdentity()
    {
        var sut = new AnonymousIdentity();
        var other = new SystemIdentity();
        Assert.False(sut.Equals(other));
        Assert.False(Equals(sut,other));
        Assert.NotEqual(sut.GetHashCode(), other.GetHashCode());
    }
}