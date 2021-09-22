using System.Collections.Generic;
using System.Linq;
using Backend.Fx.Extensions;
using Xunit;

namespace Backend.Fx.Tests.Extensions
{
    public class TheEnumerableEx
    {
        [Fact]
        public void ExecutesActionForAll()
        {
            IEnumerable<Item> enumerable = Enumerable.Range(0, 100).Select(i => new Item()).ToArray();
            enumerable.ForAll(itm => itm.Touched = true);

            Assert.All(enumerable, itm => Assert.True(itm.Touched));
        }


        private class Item
        {
            public bool Touched { get; set; }
        }
    }
}
