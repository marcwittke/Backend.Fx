using System;
using Backend.Fx.ConfigurationSettings;
using Xunit;

namespace Backend.Fx.Tests.ConfigurationSettings
{
    public class TheSettingSerializerFactory
    {
        private readonly SettingSerializerFactory _sut = new();

        [Fact]
        public void ProvidesBooleanSerializerForNullableBool()
        {
            ISettingSerializer<bool?> serializer = _sut.GetSerializer<bool?>();
            Assert.IsType<BooleanSerializer>(serializer);
        }

        [Fact]
        public void ProvidesBooleanSerializerForNullableDateTime()
        {
            ISettingSerializer<DateTime?> serializer = _sut.GetSerializer<DateTime?>();
            Assert.IsType<DateTimeSerializer>(serializer);
        }

        [Fact]
        public void ProvidesBooleanSerializerForNullableDouble()
        {
            ISettingSerializer<double?> serializer = _sut.GetSerializer<double?>();
            Assert.IsType<DoubleSerializer>(serializer);
        }

        [Fact]
        public void ProvidesBooleanSerializerForNullableInt()
        {
            ISettingSerializer<int?> serializer = _sut.GetSerializer<int?>();
            Assert.IsType<IntegerSerializer>(serializer);
        }

        [Fact]
        public void ProvidesBooleanSerializerForString()
        {
            ISettingSerializer<string> serializer = _sut.GetSerializer<string>();
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
    }
}
