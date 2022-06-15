using System.Collections.Generic;
using System.Linq;
using Backend.Fx.Extensions;
using Backend.Fx.TestUtil;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Extensions
{
    public class TheEnumerableEx : TestWithLogging
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

        public TheEnumerableEx(ITestOutputHelper output) : base(output)
        {
        }
    }
}