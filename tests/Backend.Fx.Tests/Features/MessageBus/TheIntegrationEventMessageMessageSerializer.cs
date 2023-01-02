using System;
using System.IO;
using System.Threading.Tasks;
using Backend.Fx.Features.MessageBus;
using Backend.Fx.Tests.DummyServices;
using Backend.Fx.TestUtil;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Features.MessageBus;

public class TheIntegrationEventMessageMessageSerializer : TestWithLogging
{
    private readonly IIntegrationEventMessageSerializer _sut = new IntegrationEventMessageMessageSerializer(new []{ typeof(DummyIntegrationEvent)});
    
    public TheIntegrationEventMessageMessageSerializer(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task CanRoundtrip()
    {
        var data = new DummyIntegrationEvent();
        var somePropValue = Guid.NewGuid().ToString();
        data.Properties["SomeProp"] = somePropValue;
        SerializedMessage serialized = await _sut.SerializeAsync(data);
        
        await File.WriteAllBytesAsync("message.json", serialized.MessagePayload);
        
        IIntegrationEvent deserialized = await _sut.DeserializeAsync(serialized);
        Assert.NotNull(deserialized);
        Assert.Equal(data.Id, deserialized.Id);
        Assert.True(data.Properties.ContainsKey("SomeProp"));
        Assert.Equal(somePropValue, data.Properties["SomeProp"]);
    } 
}