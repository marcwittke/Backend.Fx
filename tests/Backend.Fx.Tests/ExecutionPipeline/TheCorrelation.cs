using System;
using Backend.Fx.ExecutionPipeline;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.ExecutionPipeline;

public class TheCorrelation : TestWithLogging
{
    public TheCorrelation(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void InitializesWithRandomGuid()
    {
        var sut = new Correlation();
        Assert.NotEqual(Guid.Empty, sut.Id);
    }
    
    [Fact]
    public void CanResume()
    {
        Guid correlationIdToResume = Guid.NewGuid();
        var sut = new Correlation();
        sut.Resume(correlationIdToResume);
        Assert.Equal(correlationIdToResume, sut.Id);
    }
}