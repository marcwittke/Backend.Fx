using Backend.Fx.Patterns.IdGeneration;
using FakeItEasy;
using Xunit;

namespace Backend.Fx.Tests.Patterns.IdGeneration
{
    public class TheSequenceHiLoIdGenerator
    {
        public TheSequenceHiLoIdGenerator()
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