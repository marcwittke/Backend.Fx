namespace Backend.Fx.RandomData
{
    using System;
    using System.Collections.Generic;

    public class TestPersonGenerator
    {
        public int FemalePercentage { get; set; } = 55;

        public int GenerateCount { get; set; } = 100;

        public int MaximumAgeInDays { get; set; } = 36500;

        public int MinimumAgeInDays { get; set; } = 3650;

        public Random Random { get; set; } = TestRandom.Instance;

        public IEnumerable<TestPerson> GenerateTestPersons()
        {
            var distinctCheck = new HashSet<string>();
            for (var i = 0; i < GenerateCount; i++)
            {
                var isFemale = Random.Next(1, 100) < FemalePercentage;
                TestPerson generated = null;

                while (generated == null || distinctCheck.Contains(generated.LastName + generated.FirstName))
                {
                    generated = new TestPerson(
                        isFemale ? Names.Female.Random() : Names.Male.Random(),
                        Random.Next(100) < 30
                            ? isFemale ? Names.Female.Random() : Names.Male.Random()
                            : "",
                        Names.Family.Random(),
                        Random.Next(100) < 20 ? "Dr." : "",
                        isFemale ? TestPerson.Genders.Female : TestPerson.Genders.Male,
                        DateTime.Now.AddDays(-Random.Next(MinimumAgeInDays, MaximumAgeInDays)));
                }

                distinctCheck.Add(generated.LastName + generated.FirstName);
                yield return generated;
            }
        }
    }
}