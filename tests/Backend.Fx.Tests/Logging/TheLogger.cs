using Xunit;

namespace Backend.Fx.Tests.Logging
{
    using System;
    using FakeItEasy;
    using Fx.Logging;

    public class TheLogger
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly InvalidOperationException _exception = new InvalidOperationException("whatever");

        public TheLogger()
        {
            _loggerFactory = A.Fake<ILoggerFactory>();
            ILogger logger = A.Fake<ILogger>();

            A.CallTo(() => _loggerFactory.Create(A<string>.Ignored)).Returns(logger);
            A.CallTo(() => logger.DebugDuration(A<string>.Ignored)).ReturnsLazily((string activity) => new DurationLogger(s => logger.Debug(s), activity));
            A.CallTo(() => logger.InfoDuration(A<string>.Ignored)).ReturnsLazily((string activity) => new DurationLogger(s => logger.Info(s), activity));
            A.CallTo(() => logger.TraceDuration(A<string>.Ignored)).ReturnsLazily((string activity) => new DurationLogger(s => logger.Trace(s), activity));
        }

        [Fact]
        public void CanLogFatal()
        {
            string msg = "the log message";
            var logger = _loggerFactory.Create("aaa");
            logger.Fatal(msg);

            A.CallTo(() => logger.Fatal(A<string>.That.Matches(s => s == msg))).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void CanLogFatalWithException()
        {
            string msg = "the log message";
            var logger = _loggerFactory.Create("aaa");
            logger.Fatal(_exception, msg);

            A.CallTo(() => logger.Fatal(A<Exception>.That.Matches(ex => ex == _exception), A<string>.That.Matches(s => s == msg))).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void CanLogError()
        {
            string msg = "the log message";
            var logger = _loggerFactory.Create("aaa");
            logger.Error(msg);

            A.CallTo(() => logger.Error(A<string>.That.Matches(s => s == msg))).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void CanLogErrorWithException()
        {
            string msg = "the log message";
            var logger = _loggerFactory.Create("aaa");
            logger.Error(_exception, msg);

            A.CallTo(() => logger.Error(A<Exception>.That.Matches(ex => ex == _exception), A<string>.That.Matches(s => s == msg))).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void CanLogWarning()
        {
            string msg = "the log message";
            var logger = _loggerFactory.Create("aaa");
            logger.Warn(msg);

            A.CallTo(() => logger.Warn(A<string>.That.Matches(s => s == msg))).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void CanLogWarnWithException()
        {
            string msg = "the log message";
            var logger = _loggerFactory.Create("aaa");
            logger.Warn(_exception, msg);

            A.CallTo(() => logger.Warn(A<Exception>.That.Matches(ex => ex == _exception), A<string>.That.Matches(s => s == msg))).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void CanLogInfo()
        {
            string msg = "the log message";
            var logger = _loggerFactory.Create("aaa");
            logger.Info(msg);

            A.CallTo(() => logger.Info(A<string>.That.Matches(s => s == msg))).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void CanLogInfoWithException()
        {
            string msg = "the log message";
            var logger = _loggerFactory.Create("aaa");
            logger.Info(_exception, msg);

            A.CallTo(() => logger.Info(A<Exception>.That.Matches(ex => ex == _exception), A<string>.That.Matches(s => s == msg))).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void CanLogInfoDuration()
        {
            string msg = "the log message";
            var logger = _loggerFactory.Create("aaa");
            logger.InfoDuration(msg).Dispose();

            A.CallTo(() => logger.Info(A<string>.That.Matches(s => s == msg))).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => logger.Info(A<string>.That.Matches(s => s.StartsWith(msg + " - Duration:")))).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void CanLogDebug()
        {
            string msg = "the log message";
            var logger = _loggerFactory.Create("aaa");
            logger.Debug(msg);

            A.CallTo(() => logger.Debug(A<string>.That.Matches(s => s == msg))).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void CanLogDebugWithException()
        {
            string msg = "the log message";
            var logger = _loggerFactory.Create("aaa");
            logger.Debug(_exception, msg);

            A.CallTo(() => logger.Debug(A<Exception>.That.Matches(ex => ex == _exception), A<string>.That.Matches(s => s == msg))).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void CanLogDebugDuration()
        {
            string msg = "the log message";
            var logger = _loggerFactory.Create("aaa");
            logger.DebugDuration(msg).Dispose();

            A.CallTo(() => logger.Debug(A<string>.That.Matches(s => s == msg))).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => logger.Debug(A<string>.That.Matches(s => s.StartsWith(msg+" - Duration:")))).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void CanLogTrace()
        {
            string msg = "the log message";
            var logger = _loggerFactory.Create("aaa");
            logger.Trace(msg);

            A.CallTo(() => logger.Trace(A<string>.That.Matches(s => s == msg))).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void CanLogTraceWithException()
        {
            string msg = "the log message";
            var logger = _loggerFactory.Create("aaa");
            logger.Trace(_exception, msg);

            A.CallTo(() => logger.Trace(A<Exception>.That.Matches(ex => ex == _exception), A<string>.That.Matches(s => s == msg))).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void CanLogTraceDuration()
        {
            string msg = "the log message";
            var logger = _loggerFactory.Create("aaa");
            logger.TraceDuration(msg).Dispose();

            A.CallTo(() => logger.Trace(A<string>.That.Matches(s => s == msg))).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => logger.Trace(A<string>.That.Matches(s => s.StartsWith(msg + " - Duration:")))).MustHaveHappened(Repeated.Exactly.Once);
        }
    }
}