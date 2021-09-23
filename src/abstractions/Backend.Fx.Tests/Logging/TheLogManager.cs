using System;
using System.Security;
using Backend.Fx.Logging;
using FakeItEasy;
using Xunit;

namespace Backend.Fx.Tests.Logging
{
    public class TheDefaultLogManager
    {
        public TheDefaultLogManager()
        {
            // it's a static variable, ensure to reset it to default now:
            LogManager.Initialize(new DebugLoggerFactory());
        }

        [Fact]
        public void DoesNotThrowOnZeroConfig()
        {
            Exception ex = new SecurityException("very bad");
            var msg = "the log message";
            ILogger[] loggers =
            {
                LogManager.Create<TheLogManager>(),
                LogManager.Create(typeof(TheLogManager)),
                LogManager.Create("Backend.Fx.Tests.Logging.TheLogManager")
            };

            foreach (var logger in loggers)
            {
                logger.Fatal(ex);
                logger.Fatal(ex, msg);
                logger.Fatal(msg);
                logger.Error(ex);
                logger.Error(ex, msg);
                logger.Error(msg);
                logger.Warn(ex);
                logger.Warn(ex, msg);
                logger.Warn(msg);
                logger.Info(ex);
                logger.Info(ex, msg);
                logger.Info(msg);
                logger.Debug(ex);
                logger.Debug(ex, msg);
                logger.Debug(msg);
                logger.Trace(ex);
                logger.Trace(ex, msg);
                logger.Trace(msg);

                logger.TraceDuration(msg).Dispose();
                logger.DebugDuration(msg).Dispose();
                logger.InfoDuration(msg).Dispose();
            }
        }
    }


    public class TheLogManager
    {
        private readonly ILoggerFactory _loggerFactory = A.Fake<ILoggerFactory>();

        public TheLogManager()
        {
            var logger = A.Fake<ILogger>();
            A.CallTo(() => _loggerFactory.Create<TheLogManager>()).Returns(logger);
            LogManager.Initialize(_loggerFactory);
        }

        [Fact]
        public void CanBeginActivity()
        {
            LogManager.BeginActivity();
            A.CallTo(() => _loggerFactory.BeginActivity(A<int>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void CanShutdown()
        {
            LogManager.Shutdown();
            A.CallTo(() => _loggerFactory.Shutdown()).MustHaveHappenedOnceExactly();
        }
    }
}
