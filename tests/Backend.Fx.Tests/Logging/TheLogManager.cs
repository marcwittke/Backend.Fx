namespace Backend.Fx.Tests.Logging
{
    using System;
    using System.Security;
    using FakeItEasy;
    using Fx.Logging;
    using Xunit;

    public class TheLogManager
    {
        private readonly ILoggerFactory loggerFactory;

        public TheLogManager()
        {
            loggerFactory = A.Fake<ILoggerFactory>();
            var logger = A.Fake<ILogger>();

            A.CallTo(() => loggerFactory.Create(A<string>.Ignored)).Returns(logger);
        }

        [Fact]
        public void DoesNotThrowOnZeroConfig()
        {
            Exception ex = new SecurityException("very bad");
            string msg = "the log message";
            ILogger logger = LogManager.Create<TheLogManager>();
            
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

        [Fact]
        public void TakesTypeFullNameAsLoggerName()
        {
            LogManager.Initialize(loggerFactory);
            LogManager.Create<TheLogManager>();
            A.CallTo(() => loggerFactory.Create(A<string>.That.Matches(s => s == "Backend.Fx.Tests.Logging.TheLogManager"))).MustHaveHappened(Repeated.Exactly.Once);
        }
    }
}
