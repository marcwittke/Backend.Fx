using System;
using Backend.Fx.Exceptions;
using Xunit;

namespace Backend.Fx.Tests.Exceptions;

public class TheTooManyRequestsException
{
    [Fact]
    public void CanBeInstantiated()
    {
        var exception1 = new TooManyRequestsException(5);
        var exception2 = new TooManyRequestsException(5, "With a message");
        var exception3 = new TooManyRequestsException(5, "With a message and an inner", new Exception());
    }

    [Fact]
    public void KeepsRetryAfter()
    {
        var exception1 = new TooManyRequestsException(5);
        Assert.Equal(5, exception1.RetryAfter);
    }
}