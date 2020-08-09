using System;
using Backend.Fx.Logging;
using FakeItEasy;
using Xunit;

namespace Backend.Fx.Tests.Logging
{
    public class TheLogger
    {
        public TheLogger()
        {
            _loggerFactory = A.Fake<ILoggerFactory>();
            var logger = A.Fake<ILogger>();

            A.CallTo(() => _loggerFactory.Create(A<string>.Ignored)).Returns(logger);
            A.CallTo(() => logger.DebugDuration(A<string>.Ignored)).ReturnsLazily((string activity) => new DurationLogger(s => logger.Debug(s), activity));
            A.CallTo(() => logger.InfoDuration(A<string>.Ignored)).ReturnsLazily((string activity) => new DurationLogger(s => logger.Info(s), activity));
            A.CallTo(() => logger.TraceDuration(A<string>.Ignored)).ReturnsLazily((string activity) => new DurationLogger(s => logger.Trace(s), activity));
        }

        private readonly ILoggerFactory _loggerFactory;
        private readonly InvalidOperationException _exception = new InvalidOperationException("whatever");

        [Fact]
        public void CanLogDebug()
        {
            var msg = "the log message";
            ILogger logger = _loggerFactory.Create("aaa");
            logger.Debug(msg);

            A.CallTo(() => logger.Debug(A<string>.That.Matches(s => s == msg))).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void CanLogDebugDuration()
        {
            var msg = "the log message";
            ILogger logger = _loggerFactory.Create("aaa");
            logger.DebugDuration(msg).Dispose();

            A.CallTo(() => logger.Debug(A<string>.That.Matches(s => s == msg))).MustHaveHappenedOnceExactly();
            A.CallTo(() => logger.Debug(A<string>.That.Matches(s => s.StartsWith(msg + " - Duration:")))).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void CanLogDebugWithException()
        {
            var msg = "the log message";
            ILogger logger = _loggerFactory.Create("aaa");
            logger.Debug(_exception, msg);

            A.CallTo(() => logger.Debug(A<Exception>.That.Matches(ex => ex == _exception), A<string>.That.Matches(s => s == msg))).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void CanLogError()
        {
            var msg = "the log message";
            ILogger logger = _loggerFactory.Create("aaa");
            logger.Error(msg);

            A.CallTo(() => logger.Error(A<string>.That.Matches(s => s == msg))).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void CanLogErrorWithException()
        {
            var msg = "the log message";
            ILogger logger = _loggerFactory.Create("aaa");
            logger.Error(_exception, msg);

            A.CallTo(() => logger.Error(A<Exception>.That.Matches(ex => ex == _exception), A<string>.That.Matches(s => s == msg))).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void CanLogFatal()
        {
            var msg = "the log message";
            ILogger logger = _loggerFactory.Create("aaa");
            logger.Fatal(msg);

            A.CallTo(() => logger.Fatal(A<string>.That.Matches(s => s == msg))).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void CanLogFatalWithException()
        {
            var msg = "the log message";
            ILogger logger = _loggerFactory.Create("aaa");
            logger.Fatal(_exception, msg);

            A.CallTo(() => logger.Fatal(A<Exception>.That.Matches(ex => ex == _exception), A<string>.That.Matches(s => s == msg))).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void CanLogInfo()
        {
            var msg = "the log message";
            ILogger logger = _loggerFactory.Create("aaa");
            logger.Info(msg);

            A.CallTo(() => logger.Info(A<string>.That.Matches(s => s == msg))).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void CanLogInfoDuration()
        {
            var msg = "the log message";
            ILogger logger = _loggerFactory.Create("aaa");
            logger.InfoDuration(msg).Dispose();

            A.CallTo(() => logger.Info(A<string>.That.Matches(s => s == msg))).MustHaveHappenedOnceExactly();
            A.CallTo(() => logger.Info(A<string>.That.Matches(s => s.StartsWith(msg + " - Duration:")))).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void CanLogInfoWithException()
        {
            var msg = "the log message";
            ILogger logger = _loggerFactory.Create("aaa");
            logger.Info(_exception, msg);

            A.CallTo(() => logger.Info(A<Exception>.That.Matches(ex => ex == _exception), A<string>.That.Matches(s => s == msg))).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void CanLogTrace()
        {
            var msg = "the log message";
            ILogger logger = _loggerFactory.Create("aaa");
            logger.Trace(msg);

            A.CallTo(() => logger.Trace(A<string>.That.Matches(s => s == msg))).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void CanLogTraceDuration()
        {
            var msg = "the log message";
            ILogger logger = _loggerFactory.Create("aaa");
            logger.TraceDuration(msg).Dispose();

            A.CallTo(() => logger.Trace(A<string>.That.Matches(s => s == msg))).MustHaveHappenedOnceExactly();
            A.CallTo(() => logger.Trace(A<string>.That.Matches(s => s.StartsWith(msg + " - Duration:")))).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void CanLogTraceWithException()
        {
            var msg = "the log message";
            ILogger logger = _loggerFactory.Create("aaa");
            logger.Trace(_exception, msg);

            A.CallTo(() => logger.Trace(A<Exception>.That.Matches(ex => ex == _exception), A<string>.That.Matches(s => s == msg))).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void CanLogWarning()
        {
            var msg = "the log message";
            ILogger logger = _loggerFactory.Create("aaa");
            logger.Warn(msg);

            A.CallTo(() => logger.Warn(A<string>.That.Matches(s => s == msg))).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void CanLogWarnWithException()
        {
            var msg = "the log message";
            ILogger logger = _loggerFactory.Create("aaa");
            logger.Warn(_exception, msg);

            A.CallTo(() => logger.Warn(A<Exception>.That.Matches(ex => ex == _exception), A<string>.That.Matches(s => s == msg))).MustHaveHappenedOnceExactly();
        }
    }
}