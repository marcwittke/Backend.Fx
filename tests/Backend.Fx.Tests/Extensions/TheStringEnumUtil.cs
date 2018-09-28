using JetBrains.Annotations;

namespace Backend.Fx.Tests.Extensions
{
    using System;
    using Fx.Extensions;
    using Xunit;

    public enum AnEnum
    {
        One,
        [UsedImplicitly]
        Two,
        [UsedImplicitly]
        Three
    }

    public class TheStringEnumUtil
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
        public void ParsesCaseSensitive()
        {
            Assert.Equal(AnEnum.One, "One".Parse<AnEnum>());
            Assert.Throws<ArgumentException>(() => "one".Parse<AnEnum>());
            Assert.Throws<ArgumentException>(() => "ONE".Parse<AnEnum>());
        }

        [Fact]
        public void ThrowsOnInvalidString()
        {
            Assert.Throws<ArgumentException>(() => "whatever".Parse<AnEnum>());
        }
    }
}
