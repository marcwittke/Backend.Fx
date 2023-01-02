using System;
using Backend.Fx.Exceptions;
using Xunit;

namespace Backend.Fx.Tests.Exceptions;

public class TheUnprocessableException
{
    [Fact]
    public void CanBeInstantiated()
    {
        var exception1 = new UnprocessableException();
        var exception2 = new UnprocessableException("With a message");
        var exception3 = new UnprocessableException("With a message and an inner", new Exception());
    }
}