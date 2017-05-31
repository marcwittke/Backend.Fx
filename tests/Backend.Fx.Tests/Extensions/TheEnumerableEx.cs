namespace Backend.Fx.Tests.Extensions
{
    using System.Collections.Generic;
    using System.Linq;
    using Fx.Extensions;
    using NLogLogging;
    using Xunit;

    public class TheEnumerableEx : IClassFixture<NLogLoggingFixture>
    {
        private class Item
        {
            public bool Touched { get; set; }
        }

        [Fact]
        public void ExecutesActionForAll()
        {
            IEnumerable<Item> enumerable = Enumerable.Range(0, 100).Select(i => new Item()).ToArray();
            enumerable.ForAll(itm => itm.Touched = true);

            Assert.All(enumerable, itm => Assert.True(itm.Touched));
        }
    }
}
