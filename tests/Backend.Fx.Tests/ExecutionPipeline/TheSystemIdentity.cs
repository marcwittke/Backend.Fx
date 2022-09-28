using Backend.Fx.ExecutionPipeline;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.ExecutionPipeline;

public class TheSystemIdentity : TestWithLogging
{
    public TheSystemIdentity(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void HasCorrectName()
    {
        var sut = new SystemIdentity();
        Assert.Equal("SYSTEM", sut.Name);
    }
    
    [Fact]
    public void IsAuthenticated()
    {
        var sut = new SystemIdentity();
        Assert.True(sut.IsAuthenticated);
    }
    
    [Fact]
    public void HasInternalAuthenticationType()
    {
        var sut = new SystemIdentity();
        Assert.Equal("Internal", sut.AuthenticationType);
    }
    
    [Fact]
    public void EqualsOtherSystemIdentity()
    {
        var sut = new SystemIdentity();
        var other = new SystemIdentity();
        Assert.True(sut.Equals(other));
        Assert.True(Equals(sut,other));
        Assert.Equal(sut.GetHashCode(), other.GetHashCode());
    }
    
    [Fact]
    public void DoesNotEqualOtherIdentity()
    {
        var sut = new SystemIdentity();
        var other = new AnonymousIdentity();
        Assert.False(sut.Equals(other));
        Assert.False(Equals(sut,other));
        Assert.NotEqual(sut.GetHashCode(), other.GetHashCode());
    }
}