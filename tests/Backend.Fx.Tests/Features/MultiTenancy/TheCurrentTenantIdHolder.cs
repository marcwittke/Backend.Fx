using Backend.Fx.Features.MultiTenancy;
using Backend.Fx.TestUtil;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Features.MultiTenancy;

public class TheCurrentTenantIdHolder : TestWithLogging
{
    public TheCurrentTenantIdHolder(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void CanBeCreatedEmpty()
    {
        var sut = new CurrentTenantIdHolder();
        Assert.False(sut.Current.HasValue);
    }
    
    [Fact]
    public void CanBeCreatedFromTenantId()
    {
        var sut = CurrentTenantIdHolder.Create(new TenantId(234));
        Assert.Equal(234, sut.Current.Value);
    }
    
    [Fact]
    public void CanBeCreatedFromInteger()
    {
        var sut = CurrentTenantIdHolder.Create(234);
        Assert.Equal(234, sut.Current.Value);
    }
}