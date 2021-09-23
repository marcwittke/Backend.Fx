using Backend.Fx.BuildingBlocks;
using Xunit;

namespace Backend.Fx.Tests.BuildingBlocks
{
    public class TestIdentified : Identified
    {
        public TestIdentified(int id)
        {
            Id = id;
        }
    }


    public class TheIdentified
    {
        [Fact]
        public void IsEquatable()
        {
            var identified1 = new TestIdentified(1);
            var identified1Clone = new TestIdentified(1);
            var identified2 = new TestIdentified(2);
            Identified stillNull = null;

            Assert.True(identified1.Equals(identified1));
            Assert.True(identified1.Equals(identified1Clone));
            Assert.False(identified1.Equals(identified2));
            Assert.False(identified1.Equals(stillNull));
        }
    }
}
