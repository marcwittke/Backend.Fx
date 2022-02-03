using System.Linq;
using Backend.Fx.Patterns.IdGeneration;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Patterns.IdGeneration
{
    public class TheHiLoIdGenerator : TestWithLogging
    {
        private readonly HiLoIdGenerator _sut = new InMemoryHiLoIdGenerator(1, 100);

        private class IdConsument
        {
            public int[] Ids { get; private set; }

            public void GetIds(int count, IIdGenerator idGenerator)
            {
                Ids = new int[count];
                for (var i = 0; i < count; i++) Ids[i] = idGenerator.NextId();
            }
        }

        [Fact]
        public void AllowsMultipleThreadsToGetIds()
        {
            const int consumentCount = 50;
            const int idCountPerConsument = 1000;
            var idConsuments = new IdConsument[consumentCount];

            for (var i = 0; i < consumentCount; i++) idConsuments[i] = new IdConsument();

            idConsuments.AsParallel().ForAll(idConsument => { idConsument.GetIds(idCountPerConsument, _sut); });

            var allIds = idConsuments.SelectMany(idConsument => idConsument.Ids).ToArray();
            Assert.Equal(consumentCount * idCountPerConsument, allIds.Length);
            Assert.Equal(consumentCount * idCountPerConsument, allIds.Distinct().Count());
            Assert.Equal(consumentCount * idCountPerConsument + 1, _sut.NextId());
        }

        [Fact]
        public void StartsWithInitialValueAndCountsUp()
        {
            for (var i = 1; i < 1000; i++) Assert.Equal(i, _sut.NextId());
        }

        public TheHiLoIdGenerator(ITestOutputHelper output) : base(output)
        {
        }
    }

    public class InMemoryHiLoIdGenerator : HiLoIdGenerator
    {
        private readonly object _synclock = new object();
        private int _nextBlockStart;

        public InMemoryHiLoIdGenerator(int start, int increment)
        {
            _nextBlockStart = start;
            BlockSize = increment;
        }

        protected override int BlockSize { get; }

        protected override int GetNextBlockStart()
        {
            lock (_synclock)
            {
                // this simulates the behavior of a SQL sequence for example
                var result = _nextBlockStart;
                _nextBlockStart += BlockSize;
                return result;
            }
        }
    }
}