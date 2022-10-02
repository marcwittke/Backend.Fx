using System.Collections.Generic;
using Backend.Fx.Domain;
using Backend.Fx.Features.IdGeneration;
using Bogus;
using JetBrains.Annotations;

namespace Backend.Fx.EfCore6Persistence.Tests.DummyAggregates;

/// <summary>
/// An aggregate that contains a value type: Address
/// </summary>
public sealed class Supplier : Identified<int>, IAggregateRoot<int>
{
    [UsedImplicitly]
    private Supplier() { }
    
    public Supplier(int id, string name, Address postalAddress) : base(id)
    {
        Id = id;
        Name = name;
        PostalAddress = postalAddress;
    }
    
    public string  Name { get; [UsedImplicitly] init; }

    public Address PostalAddress { get; [UsedImplicitly] init; }
    
    public static Supplier CreateNewSupplier(IEntityIdGenerator entityIdGenerator)
    {
        var faker = new Faker();

        var supplier = new Supplier(entityIdGenerator.NextId(), faker.Company.CompanyName(), new Address(
            faker.Address.StreetAddress(), faker.Address.SecondaryAddress(), faker.Address.City(),
            faker.Address.ZipCode(), faker.Address.State(), faker.Address.Country()));

        return supplier;
    }
}

public class Address : ValueObject
{
    [UsedImplicitly]
    private Address()
    { }

    public Address(string line1, string line2, string city, string postalCode, string state, string country)
    {
        Line1 = line1;
        Line2 = line2;
        City = city;
        PostalCode = postalCode;
        State = state;
        Country = country;
    }

    public string Line1 { get; init; }
    public string Line2 { get; init; }
    public string City { get; init; }
    public string PostalCode { get; init; }
    public string State { get; init; }
    public string Country { get; init; }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Line1;
        yield return Line2;
        yield return City;
        yield return PostalCode;
        yield return State;
        yield return Country;
    }
}