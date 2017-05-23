namespace Backend.Fx.RandomData
{
    public class TestAddress
    {
        public TestAddress(string street, string number, string postalCode, string city)
        {
            Street = street;
            Number = number;
            PostalCode = postalCode;
            City = city;
        }

        public string Street { get; }

        public string Number { get; }

        public string PostalCode { get; }

        public string City { get; }
    }
}