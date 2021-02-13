﻿using System;
using System.Security;
using Backend.Fx.Logging;
using FakeItEasy;
using Xunit;

namespace Backend.Fx.Tests.Logging
{
    public class TheLogManager
    {
        public TheLogManager()
        {
            var loggerFactory = A.Fake<ILoggerFactory>();
            var logger = A.Fake<ILogger>();

            A.CallTo(() => loggerFactory.Create<TheLogManager>()).Returns(logger);
        }

        [Fact]
        public void DoesNotThrowOnZeroConfig()
        {
            Exception ex = new SecurityException("very bad");
            var msg = "the log message";
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
    }
}