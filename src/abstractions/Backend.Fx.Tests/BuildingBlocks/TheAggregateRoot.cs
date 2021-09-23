using System;
using System.Collections.Generic;
using Backend.Fx.BuildingBlocks;
using Backend.Fx.RandomData;
using JetBrains.Annotations;
using Xunit;

namespace Backend.Fx.Tests.BuildingBlocks
{
    public class TheAggregateRoot
    {
        private static int _nextId;

        [Fact]
        public void ChangedByPropertyIsChoppedAt100Chars()
        {
            var now = DateTime.Now;
            var sut = new TestAggregateRoot(_nextId++, "gaga");
            string moreThanHundred = Letters.RandomLowerCase(110);
            sut.SetModifiedProperties(moreThanHundred, now);
            Assert.Equal(moreThanHundred.Substring(0, 99) + "…", sut.ChangedBy);
        }

        [Fact]
        public void ChangedByPropertyIsStoredCorrectly()
        {
            var now = DateTime.Now;
            var sut = new TestAggregateRoot(_nextId++, "gaga");
            sut.SetModifiedProperties("me", now);
            Assert.Equal("me", sut.ChangedBy);
            Assert.Null(sut.CreatedBy);
        }

        [Fact]
        public void ChangedOnPropertyIsStoredCorrectly()
        {
            var now = DateTime.Now;
            var sut = new TestAggregateRoot(_nextId++, "gaga");
            sut.SetModifiedProperties("me", now);
            Assert.Equal(now, sut.ChangedOn);
            Assert.Equal(default, sut.CreatedOn);
        }

        [Fact]
        public void CreatedByPropertyIsChoppedAt100Chars()
        {
            var now = DateTime.Now;
            var sut = new TestAggregateRoot(_nextId++, "gaga");
            string moreThanHundred = Letters.RandomLowerCase(110);
            sut.SetCreatedProperties(moreThanHundred, now);
            Assert.Equal(moreThanHundred.Substring(0, 99) + "…", sut.CreatedBy);
        }

        [Fact]
        public void CreatedByPropertyIsStoredCorrectly()
        {
            var now = DateTime.Now;
            var sut = new TestAggregateRoot(_nextId++, "gaga");
            sut.SetCreatedProperties("me", now);
            Assert.Equal("me", sut.CreatedBy);
            Assert.Null(sut.ChangedBy);
        }

        [Fact]
        public void CreatedOnPropertyIsStoredCorrectly()
        {
            var now = DateTime.Now;
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
            Assert.Throws<ArgumentException>(() => sut.SetModifiedProperties("", DateTime.Now));
        }

        [Fact]
        public void ThrowsGivenEmptyCreatedBy()
        {
            var sut = new TestAggregateRoot(_nextId++, "gaga");
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentException>(() => sut.SetCreatedProperties("", DateTime.Now));
        }

        [Fact]
        public void ThrowsGivenNullChangedBy()
        {
            var sut = new TestAggregateRoot(_nextId++, "gaga");
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => sut.SetModifiedProperties(null, DateTime.Now));
        }

        [Fact]
        public void ThrowsGivenNullCreatedBy()
        {
            var sut = new TestAggregateRoot(_nextId++, "gaga");
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => sut.SetCreatedProperties(null, DateTime.Now));
        }


        public class TestAggregateRoot : AggregateRoot
        {
            public TestAggregateRoot(int id, string name) : base(id)
            {
                Name = name;
                Children.Add(new TestEntity("Child 1", this));
                Children.Add(new TestEntity("Child 2", this));
                Children.Add(new TestEntity("Child 3", this));
            }

            [UsedImplicitly]
            public string Name { get; private set; }

            public ISet<TestEntity> Children { get; } = new HashSet<TestEntity>();
        }


        public class TestEntity : Entity
        {
            public TestEntity(string name, TestAggregateRoot parent)
            {
                Name = name;
                Parent = parent;
            }

            [UsedImplicitly]
            public string Name { get; set; }

            [UsedImplicitly]
            public TestAggregateRoot Parent { get; set; }
        }
    }
}
