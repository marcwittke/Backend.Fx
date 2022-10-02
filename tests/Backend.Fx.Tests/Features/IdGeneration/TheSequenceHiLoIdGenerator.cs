using Backend.Fx.Features.IdGeneration;
using Backend.Fx.TestUtil;
using FakeItEasy;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Features.IdGeneration
{
    public class TheSequenceHiLoIdGenerator : TestWithLogging
    {
        public TheSequenceHiLoIdGenerator(ITestOutputHelper output): base(output)
        {
            A.CallTo(() => _sequence.Increment).Returns(10);
            _sut = new TestIdGenerator(_sequence);
        }

        private readonly ISequence _sequence = A.Fake<ISequence>();
        private readonly SequenceHiLoIdGenerator _sut;

        private class TestIdGenerator : SequenceHiLoIdGenerator
        {
            public TestIdGenerator(ISequence sequence) : base(sequence)
            {
            }
        }

        [Fact]
        public void CallsSequenceNextValueOnBlockStart()
        {
            for (var i = 0; i < 10; i++) _sut.NextId();

            A.CallTo(() => _sequence.GetNextValue()).MustHaveHappenedOnceExactly();
            _sut.NextId();
            A.CallTo(() => _sequence.GetNextValue()).MustHaveHappenedTwiceExactly();
        }
    }
}