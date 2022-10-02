using System.Threading.Tasks;
using Backend.Fx.ExecutionPipeline;
using Backend.Fx.TestUtil;
using NodaTime;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.ExecutionPipeline;

public class TheFrozenClock : TestWithLogging
{
    public TheFrozenClock(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task IsFrozen()
    {
        var sut = new FrozenClock(SystemClock.Instance);
        await Task.Delay(10);
        Assert.True(
            sut.GetCurrentInstant() <= SystemClock.Instance.GetCurrentInstant().Plus(-Duration.FromMilliseconds(9)));
    }
}