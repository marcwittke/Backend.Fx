using System;
using Backend.Fx.ExecutionPipeline;
using Backend.Fx.Extensions.MessageBus;
using Backend.Fx.TestUtil;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Patterns.DependencyInjection
{
    public class TheCorrelation : TestWithLogging
    {
        private readonly Correlation _sut = new Correlation();

        [Fact]
        public void CanResume()
        {
            var resumedCorrelationId = Guid.NewGuid();
            _sut.Resume(resumedCorrelationId);
            Assert.Equal(resumedCorrelationId, _sut.Id);
        }

        [Fact]
        public void InitializesWithId()
        {
            Assert.NotEqual(Guid.Empty, _sut.Id);
        }

        public TheCorrelation(ITestOutputHelper output) : base(output)
        {
        }
    }
}