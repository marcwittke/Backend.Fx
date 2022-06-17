using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Backend.Fx.RandomData
{
    [PublicAPI]
    public class TestPersonGenerator : Generator<TestPerson>
    {
        private const int Year = 365;
        private readonly Random _random = TestRandom.Instance;
        private readonly HashSet<string> _uniqueNames = new HashSet<string>();

        public bool EnforceUniqueNames { get; set; }
        public int FemalePercentage { get; set; } = 55;
        public int MaximumAgeInDays { get; set; } = 80 * Year;
        public int MinimumAgeInDays { get; set; } = 18 * Year;

        public static TestPerson Generate()
        {
            return new TestPersonGenerator().First();
        }

        protected override TestPerson Next()
        {
            var isFemale = _random.Next(1, 100) < FemalePercentage;
            TestPerson generated;
            do
            {
                generated = new TestPerson(
                    isFemale ? Names.Female.Random() : Names.Male.Random(),
                    _random.Next(100) < 30
                        ? isFemale ? Names.Female.Random() : Names.Male.Random()
                        : "",
                    Names.Family.Random(),
                    _random.Next(100) < 20 ? "Dr." : "",
                    isFemale ? TestPerson.Genders.Female : TestPerson.Genders.Male,
                    DateTime.UtcNow.AddDays(-_random.Next(MinimumAgeInDays, MaximumAgeInDays)).Date);
            } while (EnforceUniqueNames && _uniqueNames.Contains($"{generated.FirstName}{generated.LastName}"));

            if (EnforceUniqueNames)
            {
                _uniqueNames.Add($"{generated.FirstName}{generated.LastName}");
            }

            return generated;
        }
    }
}