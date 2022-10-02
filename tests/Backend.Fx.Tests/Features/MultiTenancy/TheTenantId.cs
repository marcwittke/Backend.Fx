using System;
using Backend.Fx.Features.MultiTenancy;
using Backend.Fx.TestUtil;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Features.MultiTenancy;

public class TheTenantId : TestWithLogging
{
    public TheTenantId(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void CanBeCreatedWithoutTenantId()
    {
        var sut = new TenantId(null);
        Assert.False(sut.HasValue);
        Assert.Throws<InvalidOperationException>(() => sut.Value);
        Assert.Throws<InvalidOperationException>(() => (int)sut);
        Assert.Equal("TenantId: NULL", sut.DebuggerDisplay);
        Assert.Equal("NULL", sut.ToString());
    }
    
    [Fact]
    public void CanBeCreatedWithTenantId()
    {
        var sut = new TenantId(333);
        Assert.True(sut.HasValue);
        Assert.Equal(333, sut.Value);
        Assert.Equal("TenantId: 333", sut.DebuggerDisplay);
        Assert.Equal("333", sut.ToString());
        Assert.Equal(333, (int)sut);
    }

    [Fact]
    public void ConsidersEqualTenantIdsAsEqualObjects()
    {
        var sut1 = new TenantId(333);
        var sut2 = (TenantId)333;
        Assert.Equal(sut1, sut2);
        Assert.True(Equals(sut1, sut2));
        Assert.True(sut1 == sut2);
        Assert.False(sut1 != sut2);
        Assert.Equal(sut1.GetHashCode(), sut2.GetHashCode());
        
        sut1 = new TenantId(null);
        sut2 = new TenantId(null);
        Assert.Equal(sut1, sut2);
        Assert.True(Equals(sut1, sut2));
        Assert.True(sut1 == sut2);
        Assert.False(sut1 != sut2);
        Assert.Equal(sut1.GetHashCode(), sut2.GetHashCode());
    }
}