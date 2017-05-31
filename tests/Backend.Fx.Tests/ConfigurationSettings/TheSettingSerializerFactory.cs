namespace Backend.Fx.Tests.ConfigurationSettings
{
    using System;
    using Fx.ConfigurationSettings;
    using NLogLogging;
    using Xunit;

    public class TheSettingSerializerFactory : IClassFixture<NLogLoggingFixture>
    {
        private readonly SettingSerializerFactory sut = new SettingSerializerFactory();

        [Fact]
        public void ProvidesBooleanSerializerForNullableBool()
        {
            var serializer = sut.GetSerializer<bool?>();
            Assert.IsType<BooleanSerializer>(serializer);
        }

        [Fact]
        public void ProvidesBooleanSerializerForNullableInt()
        {
            var serializer = sut.GetSerializer<int?>();
            Assert.IsType<IntegerSerializer>(serializer);
        }

        [Fact]
        public void ProvidesBooleanSerializerForNullableDouble()
        {
            var serializer = sut.GetSerializer<double?>();
            Assert.IsType<DoubleSerializer>(serializer);
        }

        [Fact]
        public void ProvidesBooleanSerializerForNullableDateTime()
        {
            var serializer = sut.GetSerializer<DateTime?>();
            Assert.IsType<DateTimeSerializer>(serializer);
        }

        [Fact]
        public void ProvidesBooleanSerializerForString()
        {
            var serializer = sut.GetSerializer<string>();
            Assert.IsType<StringSerializer>(serializer);
        }

        [Fact]
        public void ProvidesNoSerializerForBool()
        {
            Assert.Throws<ArgumentOutOfRangeException>(()=>sut.GetSerializer<bool>());
        }

        [Fact]
        public void ProvidesNoSerializerForInt()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => sut.GetSerializer<int>());
        }

        [Fact]
        public void ProvidesNoSerializerForDouble()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => sut.GetSerializer<double>());
        }

        [Fact]
        public void ProvidesNoSerializerForDateTime()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => sut.GetSerializer<DateTime>());
        }
    }
}
