using System;
using System.Linq;

namespace Backend.Fx.RandomData
{
    public class TestPersonGenerator : Generator<TestPerson>
    {
        private readonly Random _random = TestRandom.Instance;
        public int FemalePercentage { get; set; } = 55;
        public int MaximumAgeInDays { get; set; } = 36500;
        public int MinimumAgeInDays { get; set; } = 3650;

        public static TestAddress Generate()
        {
            return new TestAddressGenerator().First();
        }


        protected override TestPerson Next()
        {
            var isFemale = _random.Next(1, 100) < FemalePercentage;
            var generated = new TestPerson(
                isFemale ? Names.Female.Random() : Names.Male.Random(),
                _random.Next(100) < 30
                    ? isFemale ? Names.Female.Random() : Names.Male.Random()
                    : "",
                Names.Family.Random(),
                _random.Next(100) < 20 ? "Dr." : "",
                isFemale ? TestPerson.Genders.Female : TestPerson.Genders.Male,
                DateTime.Now.AddDays(-_random.Next(MinimumAgeInDays, MaximumAgeInDays)));

            return generated;
        }
    }
}