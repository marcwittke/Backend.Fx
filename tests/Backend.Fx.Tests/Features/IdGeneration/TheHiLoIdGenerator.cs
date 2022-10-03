﻿using System;
using System.Linq;
using Backend.Fx.Features.IdGeneration;
using Backend.Fx.Features.IdGeneration.InMem;
using Backend.Fx.TestUtil;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Features.IdGeneration
{
    public class TheHiLoIdGenerator : TestWithLogging
    {
        private readonly HiLoIdGenerator<int> _sut = new SequenceHiLoIntIdGenerator(new InMemorySequence(100));

        private class IdConsumer
        {
            public int[] Ids { get; private set; } = Array.Empty<int>();

            public void GetIds(int count, IIdGenerator<int> idGenerator)
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