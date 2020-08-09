using System;
using Backend.Fx.Patterns.DependencyInjection;
using Xunit;

namespace Backend.Fx.Tests.Patterns.DependencyInjection
{
    public class TheCorrelation
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
    }
}