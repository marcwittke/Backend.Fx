using System;
using Backend.Fx.Exceptions;
using Xunit;

namespace Backend.Fx.Tests.Exceptions;

public class TheConflictedException
{
    [Fact]
    public void CanBeInstantiated()
    {
        var exception1 = new ConflictedException();
        var exception2 = new ConflictedException("With a message");
        var exception3 = new ConflictedException("With a message and an inner", new Exception());
    }
}