using System.Linq;
using Backend.Fx.Patterns.IdGeneration;
using Xunit;

namespace Backend.Fx.Tests.Patterns.IdGeneration
{
    public class TheHiLoIdGenerator
    {
        private readonly HiLoIdGenerator _sut = new InMemoryHiLoIdGenerator(1, 100);

        [Fact]
        public void AllowsMultipleThreadsToGetIds()
        {
            const int consumerCount = 50;
            const int idCountPerConsumer = 1000;
            var idConsumers = new IdConsumer[consumerCount];

            for (var i = 0; i < consumerCount; i++)
            {
                idConsumers[i] = new IdConsumer();
            }

            idConsumers.AsParallel().ForAll(idConsumer => { idConsumer.GetIds(idCountPerConsumer, _sut); });

            int[] allIds = idConsumers.SelectMany(idConsumer => idConsumer.Ids).ToArray();
            Assert.Equal(consumerCount * idCountPerConsumer, allIds.Length);
            Assert.Equal(consumerCount * idCountPerConsumer, allIds.Distinct().Count());
            Assert.Equal(consumerCount * idCountPerConsumer + 1, _sut.NextId());
        }

        [Fact]
        public void StartsWithInitialValueAndCountsUp()
        {
            for (var i = 1; i < 1000; i++)
            {
                Assert.Equal(i, _sut.NextId());
            }
        }


        private class IdConsumer
        {
            public int[] Ids { get; private set; }

            public void GetIds(int count, IIdGenerator idGenerator)
            {
                Ids = new int[count];
                for (var i = 0; i < count; i++)
                {
                    Ids[i] = idGenerator.NextId();
                }
            }
        }
    }


    public class InMemoryHiLoIdGenerator : HiLoIdGenerator
    {
        private readonly object _syncLock = new();
        private int _nextBlockStart;

        public InMemoryHiLoIdGenerator(int start, int increment)
        {
            _nextBlockStart = start;
            BlockSize = increment;
        }

        protected override int BlockSize { get; }

        protected override int GetNextBlockStart()
        {
            lock (_syncLock)
            {
                // this simulates the behavior of a SQL sequence for example
                int result = _nextBlockStart;
                _nextBlockStart += BlockSize;
                return result;
            }
        }
    }
}
