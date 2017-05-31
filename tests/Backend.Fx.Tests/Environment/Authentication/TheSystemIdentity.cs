﻿namespace Backend.Fx.Tests.Environment.Authentication
{
    using Fx.Environment.Authentication;
    using NLogLogging;
    using Xunit;

    public class TheSystemIdentity : IClassFixture<NLogLoggingFixture>
    {
        [Fact]
        public void IsAuthenticated()
        {
            Assert.True(new SystemIdentity().IsAuthenticated);
        }

        [Fact]
        public void HasNameSystem()
        {
            Assert.Equal("SYSTEM", new SystemIdentity().Name);
        }

        [Fact]
        public void HasAuthenticationTypeSystemInternal()
        {
            Assert.Equal("system internal", new SystemIdentity().AuthenticationType);
        }
    }
}
