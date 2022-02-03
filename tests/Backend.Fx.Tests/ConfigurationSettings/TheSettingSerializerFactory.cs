using System;
using Backend.Fx.ConfigurationSettings;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.ConfigurationSettings
{
    public class TheSettingSerializerFactory : TestWithLogging
    {
        private readonly SettingSerializerFactory _sut = new SettingSerializerFactory();

        [Fact]
        public void ProvidesBooleanSerializerForNullableBool()
        {
            var serializer = _sut.GetSerializer<bool?>();
            Assert.IsType<BooleanSerializer>(serializer);
        }

        [Fact]
        public void ProvidesBooleanSerializerForNullableDateTime()
        {
            var serializer = _sut.GetSerializer<DateTime?>();
            Assert.IsType<DateTimeSerializer>(serializer);
        }

        [Fact]
        public void ProvidesBooleanSerializerForNullableDouble()
        {
            var serializer = _sut.GetSerializer<double?>();
            Assert.IsType<DoubleSerializer>(serializer);
        }

        [Fact]
        public void ProvidesBooleanSerializerForNullableInt()
        {
            var serializer = _sut.GetSerializer<int?>();
            Assert.IsType<IntegerSerializer>(serializer);
        }

        [Fact]
        public void ProvidesBooleanSerializerForString()
        {
            var serializer = _sut.GetSerializer<string>();
            Assert.IsType<StringSerializer>(serializer);
        }

        [Fact]
        public void ProvidesNoSerializerForBool()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _sut.GetSerializer<bool>());
        }

        [Fact]
        public void ProvidesNoSerializerForDateTime()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _sut.GetSerializer<DateTime>());
        }

        [Fact]
        public void ProvidesNoSerializerForDouble()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _sut.GetSerializer<double>());
        }

        [Fact]
        public void ProvidesNoSerializerForInt()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _sut.GetSerializer<int>());
        }

        public TheSettingSerializerFactory(ITestOutputHelper output) : base(output)
        {
        }
    }
}