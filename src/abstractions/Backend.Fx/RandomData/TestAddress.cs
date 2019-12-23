namespace Backend.Fx.RandomData
{
    public class TestAddress
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
    }
}