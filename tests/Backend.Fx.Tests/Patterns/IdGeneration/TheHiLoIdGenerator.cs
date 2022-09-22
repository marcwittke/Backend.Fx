using System.Linq;
using Backend.Fx.Domain;
using Backend.Fx.Extensions.Persistence;
using Backend.Fx.InMemoryPersistence;
using Backend.Fx.TestUtil;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Patterns.IdGeneration
{
    public class TheHiLoIdGenerator : TestWithLogging
    {
        private readonly HiLoIdGenerator _sut = new SequenceHiLoIdGenerator(new InMemorySequence(100));

        private class IdConsumer
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
            const int consumerCount = 50;
            const int idCountPerConsumer = 1000;
            var idConsumers = new IdConsumer[consumerCount];

            for (var i = 0; i < consumerCount; i++) idConsumers[i] = new IdConsumer();

            idConsumers.AsParallel().ForAll(idConsumer => { idConsumer.GetIds(idCountPerConsumer, _sut); });

            var allIds = idConsumers.SelectMany(idConsumer => idConsumer.Ids).ToArray();
            Assert.Equal(consumerCount * idCountPerConsumer, allIds.Length);
            Assert.Equal(consumerCount * idCountPerConsumer, allIds.Distinct().Count());
            Assert.Equal(allIds.Max() + 1, _sut.NextId());
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
}