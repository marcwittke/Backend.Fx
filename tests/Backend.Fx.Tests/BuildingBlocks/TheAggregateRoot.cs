using System;
using System.Collections.Generic;
using Backend.Fx.RandomData;
using Backend.Fx.TestUtil;
using JetBrains.Annotations;
using NodaTime;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.BuildingBlocks
{
    public class TheAggregateRoot : TestWithLogging
    {
        private static int _nextId;

        public class TestAggregateRoot : AggregateRoot
        {
            public TestAggregateRoot(int id, string name) : base(id)
            {
                Name = name;
                Children.Add(new TestEntity("Child 1", this));
                Children.Add(new TestEntity("Child 2", this));
                Children.Add(new TestEntity("Child 3", this));
            }

            [UsedImplicitly] public string Name { get; private set; }

            public ISet<TestEntity> Children { get; } = new HashSet<TestEntity>();
        }

        public class TestEntity : Entity
        {
            public TestEntity(string name, TestAggregateRoot parent)
            {
                Name = name;
                Parent = parent;
            }

            [UsedImplicitly] public string Name { get; set; }

            [UsedImplicitly] public TestAggregateRoot Parent { get; set; }
        }

        [Fact]
        public void ChangedByPropertyIsChoppedAt100Chars()
        {
            Instant now = SystemClock.Instance.GetCurrentInstant();
            var sut = new TestAggregateRoot(_nextId++, "gaga");
            var moreThanHundred = Letters.RandomLowerCase(110);
            sut.SetModifiedProperties(moreThanHundred, now);
            Assert.Equal(moreThanHundred.Substring(0, 99) + "…", sut.ChangedBy);
        }

        [Fact]
        public void ChangedByPropertyIsStoredCorrectly()
        {
            Instant now = SystemClock.Instance.GetCurrentInstant();
            var sut = new TestAggregateRoot(_nextId++, "gaga");
            sut.SetModifiedProperties("me", now);
            Assert.Equal("me", sut.ChangedBy);
            Assert.Null(sut.CreatedBy);
        }

        [Fact]
        public void ChangedOnPropertyIsStoredCorrectly()
        {
            Instant now = SystemClock.Instance.GetCurrentInstant();
            var sut = new TestAggregateRoot(_nextId++, "gaga");
            sut.SetModifiedProperties("me", now);
            Assert.Equal(now, sut.ChangedOn);
            Assert.Equal(default, sut.CreatedOn);
        }

        [Fact]
        public void CreatedByPropertyIsChoppedAt100Chars()
        {
            Instant now = SystemClock.Instance.GetCurrentInstant();
            var sut = new TestAggregateRoot(_nextId++, "gaga");
            var moreThanHundred = Letters.RandomLowerCase(110);
            sut.SetCreatedProperties(moreThanHundred, now);
            Assert.Equal(moreThanHundred.Substring(0, 99) + "…", sut.CreatedBy);
        }

        [Fact]
        public void CreatedByPropertyIsStoredCorrectly()
        {
            Instant now = SystemClock.Instance.GetCurrentInstant();
            var sut = new TestAggregateRoot(_nextId++, "gaga");
            sut.SetCreatedProperties("me", now);
            Assert.Equal("me", sut.CreatedBy);
            Assert.Null(sut.ChangedBy);
        }

        [Fact]
        public void CreatedOnPropertyIsStoredCorrectly()
        {
            Instant now = SystemClock.Instance.GetCurrentInstant();
            var sut = new TestAggregateRoot(_nextId++, "gaga");
            sut.SetCreatedProperties("me", now);
            Assert.Equal(now, sut.CreatedOn);
            Assert.Null(sut.ChangedOn);
        }

        [Fact]
        public void ThrowsGivenEmptyChangedBy()
        {
            var sut = new TestAggregateRoot(_nextId++, "gaga");
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentException>(() => sut.SetModifiedProperties("", SystemClock.Instance.GetCurrentInstant()));
        }

        [Fact]
        public void ThrowsGivenEmptyCreatedBy()
        {
            var sut = new TestAggregateRoot(_nextId++, "gaga");
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentException>(() => sut.SetCreatedProperties("", SystemClock.Instance.GetCurrentInstant()));
        }

        [Fact]
        public void ThrowsGivenNullChangedBy()
        {
            var sut = new TestAggregateRoot(_nextId++, "gaga");
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => sut.SetModifiedProperties(null, SystemClock.Instance.GetCurrentInstant()));
        }

        [Fact]
        public void ThrowsGivenNullCreatedBy()
        {
            var sut = new TestAggregateRoot(_nextId++, "gaga");
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => sut.SetCreatedProperties(null, SystemClock.Instance.GetCurrentInstant()));
        }

        public TheAggregateRoot(ITestOutputHelper output) : base(output)
        {
        }
    }
}