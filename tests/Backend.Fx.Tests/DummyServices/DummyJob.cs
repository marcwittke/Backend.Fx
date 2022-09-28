using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Features.Jobs;
using FakeItEasy;
using JetBrains.Annotations;

namespace Backend.Fx.Tests.DummyServices;

[UsedImplicitly]
public class DummyJob : IJob
{
    private readonly IDummyJobSpy _spy;

    public DummyJob(IDummyJobSpy spy)
    {
        _spy = spy;
    }
    
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        
        await _spy.RunAsync(cancellationToken);
    }
}

public interface IDummyJobSpy : IJob
{ }