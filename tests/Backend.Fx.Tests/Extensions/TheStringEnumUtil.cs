using System;
using Backend.Fx.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Extensions
{
    public enum AnEnum
    {
        One,
        Two,
        Three
    }

    public class TheStringEnumUtil : TestWithLogging
    {
        [Theory]
        [InlineData("One")]
        [InlineData("Two")]
        [InlineData("Three")]
        public void ParsesStringsToEnums(string s)
        {
            Assert.IsType<AnEnum>(s.Parse<AnEnum>());
        }

        [Fact]
        public void ParsesCaseInsensitive()
        {
            Assert.Equal(AnEnum.One, "One".Parse<AnEnum>());
            Assert.Equal(AnEnum.Two, "two".Parse<AnEnum>());
            Assert.Equal(AnEnum.Three, "THREE".Parse<AnEnum>());
        }

        [Fact]
        public void ThrowsOnInvalidString()
        {
            Assert.Throws<ArgumentException>(() => "whatever".Parse<AnEnum>());
        }

        public TheStringEnumUtil(ITestOutputHelper output) : base(output)
        {
        }
    }
}