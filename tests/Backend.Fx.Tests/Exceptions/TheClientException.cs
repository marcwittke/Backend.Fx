using System;
using Backend.Fx.Exceptions;
using Backend.Fx.TestUtil;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Exceptions;

public class TheClientException : TestWithLogging
{
    private readonly ITestOutputHelper _output;

    public TheClientException(ITestOutputHelper output) : base(output)
    {
        _output = output;
    }

    [Fact]
    public void CanBeInstantiated()
    {
        var exception1 = new ClientException();
        var exception2 = new ClientException("With a message");
        var exception3 = new ClientException("With a message and an inner", new Exception());
    }
    
    [Fact]
    public void AllowsAddingError()
    {
        var exception = new ClientException()
            .AddError("The error message");

        Assert.Contains(exception.Errors, err => err.Key == string.Empty && err.Value[0] == "The error message");
        Assert.True(exception.HasErrors());
        Assert.NotEmpty(exception.Errors[string.Empty]);
        Assert.Contains(string.Empty, exception.Errors.Keys);
        Assert.Contains(exception.Errors.Values, val => val[0] == "The error message");
    }
    
    [Fact]
    public void AllowsAddingErrors()
    {
        var exception = new ClientException()
            .AddErrors(new [] {"The first error message", "The second error message"});

        Assert.Contains(
            exception.Errors,
            err => 
                err.Key == string.Empty 
                && err.Value[0] == "The first error message"
                && err.Value[1] == "The second error message");
        Assert.True(exception.HasErrors());
    }
    
    [Fact]
    public void AllowsAddingKeyedError()
    {
        var exception = new ClientException()
            .AddError("key", "The error message");

        Assert.Contains(exception.Errors, err => err.Key == "key" && err.Value[0] == "The error message");
        Assert.True(exception.HasErrors());
        Assert.True(exception.Errors.ContainsKey("key"));
    }
    
    [Fact]
    public void AllowsAddingKeyedErrors()
    {
        var exception = new ClientException()
            .AddErrors("key", new [] {"The first error message", "The second error message"});

        Assert.Contains(
            exception.Errors,
            err => 
                err.Key == "key" 
                && err.Value[0] == "The first error message"
                && err.Value[1] == "The second error message");
        Assert.True(exception.HasErrors());
        Assert.True(exception.Errors.ContainsKey("key"));
    }

    [Fact]
    public void ContainsErrorsInToString()
    {
        var exception = new ClientException()
            .AddErrors("key", new [] {"The first error message", "The second error message"});

        string exToString = exception.ToString();
        Assert.Contains("Errors: 1", exToString);
        Assert.Contains("key", exToString);
        Assert.Contains("[0] The first error message", exToString);
        Assert.Contains("[1] The second error message", exToString);
    }
}