using System.Collections.Generic;
using System.Linq;

namespace Backend.Fx.RandomData
{
    public class TestAddressGenerator : Generator<TestAddress>
    {
        public static TestAddress Generate()
        {
            return new TestAddressGenerator().First();
        }
        
        public override IEnumerator<TestAddress> GetEnumerator()
        {
            return new Enumerator();
        }

        private class Enumerator : GeneratingEnumerator<TestAddress>
        {
            protected override TestAddress Next()
            {
                return new TestAddress(
                    Names.Streets.Random(),
                    Numbers.RandomHouseNumber(),
                    Numbers.RandomPostalCode(),
                    Names.Cities.Random(),
                    Names.Countries.Random());
            }
        }
    }
}