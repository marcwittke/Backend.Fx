using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.ExecutionPipeline;
using Backend.Fx.TestUtil;
using Backend.Fx.Util;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Patterns.DependencyInjection
{
    public class TheCurrentTHolder : TestWithLogging
    {
        private readonly ICurrentTHolder<TenantId> _sut = new CurrentTenantIdHolder();
        private readonly TenantId _instance2 = new TenantId(2);

        [Fact]
        public void CanReplaceCurrent()
        {
            _sut.ReplaceCurrent(_instance2);
            Assert.StrictEqual(_instance2, _sut.Current);
        }

        [Fact]
        public void HoldsCurrent()
        {
            TenantId current = _sut.Current;
            Assert.False(current.HasValue);
        }

        public TheCurrentTHolder(ITestOutputHelper output) : base(output)
        {
        }
    }
}