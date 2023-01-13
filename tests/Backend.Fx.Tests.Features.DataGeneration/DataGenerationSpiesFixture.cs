using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Features.DataGeneration;
using FakeItEasy;
using JetBrains.Annotations;
using Xunit;

namespace Backend.Fx.Tests.Features.DataGeneration;

// here we have two data generators with a static fake to record calls to it. We cannot use the fakes directly with injection since 
// data generators are picked up dynamically by scanning all assemblies and registered as a collection.
// by use of the collection fixture attribute, we prevent parallel tests intervening the call count on the spies

// Since the spies are static and are picked up on every boot, these tests reside in a separate test assembly

[UsedImplicitly]
public class DataGenerationSpiesFixture 
{
    [UsedImplicitly]
    public class SomeDemoDataGenerator : IDemoDataGenerator
    {
        public static readonly IDemoDataGenerator Spy = A.Fake<IDemoDataGenerator>();
        
        public int Priority => Spy.Priority;

        public Task GenerateAsync(CancellationToken cancellationToken = default)
        {
            return Spy.GenerateAsync(cancellationToken);
        }
    }
    
    [UsedImplicitly]
    public class SomeProductiveDataGenerator : IProductiveDataGenerator
    {
        public static readonly IProductiveDataGenerator Spy = A.Fake<IProductiveDataGenerator>();

        public int Priority => Spy.Priority;

        public Task GenerateAsync(CancellationToken cancellationToken = default)
        {
            return Spy.GenerateAsync(cancellationToken);
        }
    }
}

[CollectionDefinition("Data Generation Collection")]
public class DataGenerationCollection : ICollectionFixture<DataGenerationSpiesFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}


