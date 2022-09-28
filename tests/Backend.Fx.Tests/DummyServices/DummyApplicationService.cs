using Backend.Fx.Features.DomainServices;
using JetBrains.Annotations;

namespace Backend.Fx.Tests.DummyServices;


public interface IDummyApplicationService
{
    string SayHelloToApplication();
}

[UsedImplicitly]
public class DummyApplicationService : IDummyApplicationService, IDomainService
{
    public const string Message = "Hello Application!";

    public string SayHelloToApplication()
    {
        return Message;
    }
}
