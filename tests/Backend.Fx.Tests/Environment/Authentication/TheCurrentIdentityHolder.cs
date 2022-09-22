﻿using Backend.Fx.ExecutionPipeline;
using Backend.Fx.TestUtil;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Environment.Authentication
{
    public class TheCurrentIdentityHolder : TestWithLogging
    {
        [Fact]
        public void FallsBackToInitialValueWhenReplacingWithNull()
        {
            var currentIdentityHolder = new CurrentIdentityHolder();
            currentIdentityHolder.ReplaceCurrent(null);
            Assert.Equal("ANONYMOUS", currentIdentityHolder.Current.Name);
        }

        [Fact]
        public void InitializesWithAnonymousIdentity()
        {
            var currentIdentityHolder = new CurrentIdentityHolder();
            Assert.Equal("ANONYMOUS", currentIdentityHolder.Current.Name);
        }

        [Fact]
        public void ReplacesCurrentIdentity()
        {
            var currentIdentityHolder = new CurrentIdentityHolder();
            currentIdentityHolder.ReplaceCurrent(new SystemIdentity());
            Assert.Equal("SYSTEM", currentIdentityHolder.Current.Name);
        }

        public TheCurrentIdentityHolder(ITestOutputHelper output) : base(output)
        {
        }
    }
}