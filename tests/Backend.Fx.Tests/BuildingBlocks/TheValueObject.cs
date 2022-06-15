using System;
using System.Collections.Generic;
using Backend.Fx.BuildingBlocks;
using Backend.Fx.TestUtil;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.BuildingBlocks
{
    public class TheValueObject : TestWithLogging
    {
        [Fact]
        public void IsConsideredEqualWhenAllPropertiesAreEqual()
        {
            var myValueObject1 = new MyValueObject(333, "gnarf");
            var myValueObject2 = new MyValueObject(333, "gnarf");
            Assert.True(myValueObject1.Equals(myValueObject2));
            Assert.True(myValueObject2.Equals(myValueObject1));
            Assert.True(Equals(myValueObject1, myValueObject2));

            object myDumbValueObject1 = myValueObject1;
            object myDumbValueObject2 = myValueObject2;
            Assert.True(myDumbValueObject1.Equals(myDumbValueObject2));
            Assert.True(myDumbValueObject2.Equals(myDumbValueObject1));
            Assert.True(Equals(myDumbValueObject1, myDumbValueObject2));
            
            // attention! R# warns you, though
            // ReSharper disable once PossibleUnintendedReferenceComparison
            Assert.False(myValueObject1 == myValueObject2);
        }
        
        [Fact]
        public void IsNotConsideredEqualWhenOnePropertyDoesNotMatch()
        {
            var myValueObject1 = new MyValueObject(333, "gnarf");
            var myValueObject2 = new MyValueObject(334, "gnarf");
            Assert.False(myValueObject1.Equals(myValueObject2));
            Assert.False(myValueObject2.Equals(myValueObject1));
            Assert.False(Equals(myValueObject1, myValueObject2));
            
            object myDumbValueObject1 = myValueObject1;
            object myDumbValueObject2 = myValueObject2;
            Assert.False(myDumbValueObject1.Equals(myDumbValueObject2));
            Assert.False(myDumbValueObject2.Equals(myDumbValueObject1));
            Assert.False(Equals(myDumbValueObject1, myDumbValueObject2));
            
            var myValueObject3 = new MyValueObject(333, "gnarfe");
            var myValueObject4 = new MyValueObject(333, "gnarfo");
            Assert.False(myValueObject3.Equals(myValueObject4));
            Assert.False(myValueObject4.Equals(myValueObject3));
            Assert.False(Equals(myValueObject3, myValueObject4));
        }

        [Fact]
        public void CanBeCompared()
        {
            var myValueObject1 = new MyValueObject(333, "gnarf");
            var myValueObject2 = new MyValueObject(334, "gnarf");
            var myValueObject3 = new MyValueObject(334, "gnarf");
            
            Assert.True(myValueObject1.CompareTo(myValueObject2) == -1);
            Assert.True(myValueObject2.CompareTo(myValueObject1) == 1);
            Assert.True(myValueObject2.CompareTo(myValueObject3) == 0);
            Assert.True(myValueObject3.CompareTo(myValueObject2) == 0);
            
            object myDumbValueObject1 = myValueObject1;
            object myDumbValueObject2 = myValueObject2;
            object myDumbValueObject3 = myValueObject3;
            
            Assert.True(myValueObject1.CompareTo(myDumbValueObject2) == -1);
            Assert.True(myValueObject2.CompareTo(myDumbValueObject1) == 1);
            Assert.True(myValueObject2.CompareTo(myDumbValueObject3) == 0);
            Assert.True(myValueObject3.CompareTo(myDumbValueObject2) == 0);
        }

        [Fact]
        public void DoesNotEqualNull()
        {
            var myValueObject1 = new MyValueObject(333, "gnarf");
            Assert.False(myValueObject1.Equals(null));
            Assert.False(Equals(myValueObject1, null));
        }
        
        [Fact]
        public void CanCompareToNull()
        {
            var myValueObject1 = new MyValueObject(333, "gnarf");
            Assert.Equal(1,myValueObject1.CompareTo(null));
        }

        private class MyValueObject : ComparableValueObject<MyValueObject>
        {
            private int Order { get; }
            private string Name { get; }
            

            public MyValueObject(int order, string name)
            {
                Order = order;
                Name = name;
            }
            
            protected override IEnumerable<object> GetEqualityComponents()
            {
                yield return Order;
                yield return Name;
            }

            protected override IEnumerable<IComparable> GetComparableComponents()
            {
                yield return Order;
            }
        }

        public TheValueObject(ITestOutputHelper output) : base(output)
        {
        }
    }
}