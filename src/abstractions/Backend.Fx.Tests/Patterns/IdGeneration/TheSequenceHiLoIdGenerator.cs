using Backend.Fx.Patterns.IdGeneration;
using FakeItEasy;
using Xunit;

namespace Backend.Fx.Tests.Patterns.IdGeneration
{
    public class TheSequenceHiLoIdGenerator
    {
        private readonly ISequence _sequence = A.Fake<ISequence>();
        private readonly SequenceHiLoIdGenerator _sut;

        public TheSequenceHiLoIdGenerator()
        {
            A.CallTo(() => _sequence.Increment).Returns(10);
            _sut = new SequenceHiLoIdGenerator(_sequence);
        }

        [Fact]
        public void CallsSequenceNextValueOnBlockStart()
        {
            for (var i = 0; i < 10; i++)
            {
                _sut.NextId();
            }

            A.CallTo(() => _sequence.GetNextValue()).MustHaveHappenedOnceExactly();
            _sut.NextId();
            A.CallTo(() => _sequence.GetNextValue()).MustHaveHappenedTwiceExactly();
        }
    }


    public class TheSequenceIdGenerator
    {
        private readonly ISequence _sequence = A.Fake<ISequence>();
        private readonly SequenceIdGenerator _sut;

        public TheSequenceIdGenerator()
        {
            A.CallTo(() => _sequence.Increment).Returns(1);
            _sut = new SequenceIdGenerator(_sequence);
        }

        [Fact]
        public void CallsSequenceNextValueOnBlockStart()
        {
            for (var i = 0; i < 10; i++)
            {
                _sut.NextId();
            }

            A.CallTo(() => _sequence.GetNextValue()).MustHaveHappenedANumberOfTimesMatching(i => i == 10);
            _sut.NextId();
            A.CallTo(() => _sequence.GetNextValue()).MustHaveHappenedANumberOfTimesMatching(i => i == 11);
        }
    }
}
