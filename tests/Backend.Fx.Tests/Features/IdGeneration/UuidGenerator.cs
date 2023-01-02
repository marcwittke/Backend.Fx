using System;
using Backend.Fx.Features.IdGeneration;

namespace Backend.Fx.Tests.Features.IdGeneration;

public class UuidGenerator : IIdGenerator<Guid>
{
    public Guid NextId()
    {
        return Guid.NewGuid();
    }
}