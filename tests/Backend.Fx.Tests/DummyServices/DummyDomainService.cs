using Backend.Fx.Features.DomainServices;
using JetBrains.Annotations;

namespace Backend.Fx.Tests.DummyServices;


public interface IDummyDomainService
{
    string SayHelloToDomain();
}

[UsedImplicitly]
public class DummyDomainService : IDummyDomainService, IDomainService
{
    public const string Message = "Hello Domain!"; 
    
    public string SayHelloToDomain()
    {
        return Message;
    }
}
