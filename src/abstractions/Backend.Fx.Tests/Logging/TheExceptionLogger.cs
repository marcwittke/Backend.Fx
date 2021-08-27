using System;
using Backend.Fx.Exceptions;
using Backend.Fx.Logging;
using FakeItEasy;
using Xunit;

namespace Backend.Fx.Tests.Logging
{
    public class TheExceptionLogger
    {
        private readonly ILogger _logger = A.Fake<ILogger>();
        private readonly ExceptionLogger _sut;

        public TheExceptionLogger()
        {
            _sut = new ExceptionLogger(_logger);
        }

        [Fact]
        public void LogsClientExceptionAsWarning()
        {
            _sut.LogException(new ClientException("The message"));
            
            A.CallTo(()=>_logger.Error(A<ClientException>._)).MustNotHaveHappened();
            A.CallTo(()=>_logger.Warn(A<ClientException>._)).MustHaveHappenedOnceExactly();
        }
        
        [Fact]
        public void LogsDerivatesOfClientExceptionAsWarning()
        {
            _sut.LogException(new UnprocessableException("The message"));
            
            A.CallTo(()=>_logger.Error(A<UnprocessableException>._)).MustNotHaveHappened();
            A.CallTo(()=>_logger.Warn(A<UnprocessableException>._)).MustHaveHappenedOnceExactly();
        }
        
        [Fact]
        public void LogsOtherExceptionAsError()
        {
            _sut.LogException(new InvalidOperationException("The message"));
            
            A.CallTo(()=>_logger.Warn(A<InvalidOperationException>._)).MustNotHaveHappened();
            A.CallTo(()=>_logger.Error(A<InvalidOperationException>._)).MustHaveHappenedOnceExactly();
        }
    }
}