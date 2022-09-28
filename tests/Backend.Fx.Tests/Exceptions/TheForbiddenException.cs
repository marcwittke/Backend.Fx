using System;
using Backend.Fx.Exceptions;
using Xunit;

namespace Backend.Fx.Tests.Exceptions;

public class TheForbiddenException
{
    [Fact]
    public void CanBeInstantiated()
    {
        var exception1 = new ForbiddenException();
        var exception2 = new ForbiddenException("With a message");
        var exception3 = new ForbiddenException("With a message and an inner", new Exception());
    }
}