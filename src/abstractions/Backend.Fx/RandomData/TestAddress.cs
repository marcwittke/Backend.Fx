using System.Collections.Generic;
using Backend.Fx.BuildingBlocks;

namespace Backend.Fx.RandomData
{
    public class TestAddress : ValueObject
    {
        public TestAddress(string street, string number, string postalCode, string city, string country)
        {
            Street = street;
            Number = number;
            PostalCode = postalCode;
            City = city;
            Country = country;
        }

        public string Street { get; }

        public string Number { get; }

        public string PostalCode { get; }

        public string City { get; }

        public string Country { get; }
        
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Street;
            yield return Number;
            yield return PostalCode;
            yield return City;
            yield return Country;
        }
    }
}