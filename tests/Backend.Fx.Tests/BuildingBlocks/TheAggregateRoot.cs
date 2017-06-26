namespace Backend.Fx.Tests.BuildingBlocks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Fx.BuildingBlocks;
    using RandomData;
    using Xunit;

    public class TheAggregateRoot
    {
        public class TestAggregateRoot : AggregateRoot
        {
            public TestAggregateRoot(string name)
            {
                Name = name;
                Children.Add(new TestEntity("Child 1", this));
                Children.Add(new TestEntity("Child 2", this));
                Children.Add(new TestEntity("Child 3", this));
            }

            public string Name { get; private set; }

            public ISet<TestEntity> Children { get;  } = new HashSet<TestEntity>();
        }

        public class TestEntity : Entity
        {
            public TestEntity(string name, TestAggregateRoot parent)
            {
                Name = name;
                Parent = parent;
            }

            public string Name { get; set; }
            public TestAggregateRoot Parent { get; set; }
            protected override AggregateRoot FindMyAggregateRoot()
            {
                return Parent;
            }
        }

        [Fact]
        public void CreatedByPropertyIsStoredCorrectly()
        {
            DateTime now = DateTime.Now;
            var sut = new TestAggregateRoot("gaga");
            sut.SetCreatedProperties("me", now);
            Assert.Equal("me", sut.CreatedBy);
            Assert.Equal(null, sut.ChangedBy);
        }

        [Fact]
        public void CreatedOnPropertyIsStoredCorrectly()
        {
            DateTime now = DateTime.Now;
            var sut = new TestAggregateRoot("gaga");
            sut.SetCreatedProperties("me", now);
            Assert.Equal(now, sut.CreatedOn);
            Assert.Equal(null, sut.ChangedOn);
        }

        [Fact]
        public void ChangedByPropertyIsStoredCorrectly()
        {
            DateTime now = DateTime.Now;
            var sut = new TestAggregateRoot("gaga");
            sut.SetModifiedProperties("me", now);
            Assert.Equal("me", sut.ChangedBy);
            Assert.Equal(null, sut.CreatedBy);
        }

        [Fact]
        public void ChangedOnPropertyIsStoredCorrectly()
        {
            DateTime now = DateTime.Now;
            var sut = new TestAggregateRoot("gaga");
            sut.SetModifiedProperties("me", now);
            Assert.Equal(now, sut.ChangedOn);
            Assert.Equal(default(DateTime), sut.CreatedOn);
        }

        [Fact]
        public void CreatedByPropertyIsChoppedAt100Chars()
        {
            DateTime now = DateTime.Now;
            var sut = new TestAggregateRoot("gaga");
            var moreThanHundred = Letters.RandomLowerCase(110);
            sut.SetCreatedProperties(moreThanHundred, now);
            Assert.Equal(moreThanHundred.Substring(0, 99) + "…", sut.CreatedBy);
        }

        [Fact]
        public void ModifiedIsBubblingUpFromEntityToAggregateRoot()
        {
            DateTime now = DateTime.Now;
            var sut = new TestAggregateRoot("gaga");
            sut.Children.First().SetModifiedProperties("someone", now);

            Assert.Equal("someone", sut.ChangedBy);
            Assert.Equal(now, sut.ChangedOn);
        }

        [Fact]
        public void ChangedByPropertyIsChoppedAt100Chars()
        {
            DateTime now = DateTime.Now;
            var sut = new TestAggregateRoot("gaga");
            var moreThanHundred = Letters.RandomLowerCase(110);
            sut.SetModifiedProperties(moreThanHundred, now);
            Assert.Equal(moreThanHundred.Substring(0, 99) + "…", sut.ChangedBy);
        }

        [Fact]
        public void ThrowsGivenNullCreatedBy()
        {
            var sut = new TestAggregateRoot("gaga");
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => sut.SetCreatedProperties(null, DateTime.Now));
        }

        [Fact]
        public void ThrowsGivenNullChangedBy()
        {
            var sut = new TestAggregateRoot("gaga");
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => sut.SetModifiedProperties(null, DateTime.Now));
        }

        [Fact]
        public void ThrowsGivenEmptyCreatedBy()
        {
            var sut = new TestAggregateRoot("gaga");
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentException>(() => sut.SetCreatedProperties("", DateTime.Now));
        }

        [Fact]
        public void ThrowsGivenEmptyChangedBy()
        {
            var sut = new TestAggregateRoot("gaga");
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentException>(() => sut.SetModifiedProperties("", DateTime.Now));
        }

    }
}
