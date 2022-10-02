using System;
using System.Threading.Tasks;
using Backend.Fx.ExecutionPipeline;
using Backend.Fx.Logging;
using Backend.Fx.SimpleInjectorDependencyInjection;
using Backend.Fx.TestUtil;
using FakeItEasy;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.ExecutionPipeline;

public class TheExceptionLoggingAndHandlingInvoker : TestWithLogging
{
    private readonly IBackendFxApplicationInvoker _sut;
    private readonly IExceptionLogger _exceptionLogger = A.Fake<IExceptionLogger>();


    public TheExceptionLoggingAndHandlingInvoker(ITestOutputHelper output) : base(output)
    {
        var application = new BackendFxApplication(
            new SimpleInjectorCompositionRoot(), 
            _exceptionLogger, 
            GetType().Assembly);
        _sut = new ExceptionLoggingAndHandlingInvoker(_exceptionLogger, application.Invoker);
    }


    [Fact]
    public void SwallowsExceptions()
    {
        _sut.InvokeAsync(sp => Task.CompletedTask);
        _sut.InvokeAsync(sp => throw new DivideByZeroException());
    }
}