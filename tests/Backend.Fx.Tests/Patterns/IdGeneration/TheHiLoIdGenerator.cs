namespace Backend.Fx.Tests.Patterns.IdGeneration
{
    using System.Linq;
    using Fx.Patterns.IdGeneration;
    using Xunit;

    public class TheHiLoIdGenerator
    {
        private readonly HiLoIdGenerator _sut = new InMemoryHiLoIdGenerator(1, 100);

        [Fact]
        public void StartsWithInitialValueAndCountsUp()
        {
            for (int i = 1; i < 1000; i++)
            {
                Assert.Equal(i, _sut.NextId());
            }
        }

        [Fact]
        public void AllowsMultipleThreadsToGetIds()
        {
            const int consumentCount = 50;
            const int idCountPerConsument = 1000;
            IdConsument[] idConsuments = new IdConsument[consumentCount];

            for (int i = 0; i < consumentCount; i++)
            {
                idConsuments[i] = new IdConsument();
            }
            
            idConsuments.AsParallel().ForAll(idConsument=> { idConsument.GetIds(idCountPerConsument, _sut); });

            int[] allIds = idConsuments.SelectMany(idConsument => idConsument.Ids).ToArray();
            Assert.Equal(consumentCount*idCountPerConsument, allIds.Length);
            Assert.Equal(consumentCount*idCountPerConsument, allIds.Distinct().Count());
            Assert.Equal(consumentCount * idCountPerConsument + 1,_sut.NextId());
        }

        private class IdConsument
        {
            public int[] Ids { get; private set; }

            public void GetIds(int count, IIdGenerator idGenerator)
            {
                Ids = new int[count];
                for (int i = 0; i < count; i++)
                {
                    Ids[i] = idGenerator.NextId();
                }
            }
        }
    }

    public class InMemoryHiLoIdGenerator : HiLoIdGenerator
    {
        private readonly object _synclock = new object();
        private int _nextBlockStart;

        public InMemoryHiLoIdGenerator(int start, int increment)
        {
            _nextBlockStart = start;
            Increment = increment;
        }

        protected override int GetNextBlockStart()
        {
            lock (_synclock)
            {
                // this simulates the behavior of a SQL sequence for example
                int result = _nextBlockStart;
                _nextBlockStart += Increment;
                return result;
            }
        }

        protected override int Increment { get; }
    }
}
