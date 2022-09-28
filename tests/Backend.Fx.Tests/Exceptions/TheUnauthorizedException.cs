using System;
using Backend.Fx.Exceptions;
using Xunit;

namespace Backend.Fx.Tests.Exceptions;

public class TheUnauthorizedException
{
    [Fact]
    public void CanBeInstantiated()
    {
        var exception1 = new UnauthorizedException();
        var exception2 = new UnauthorizedException("With a message");
        var exception3 = new UnauthorizedException("With a message and an inner", new Exception());
    }
}