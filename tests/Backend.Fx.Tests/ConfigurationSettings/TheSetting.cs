using System;
using Backend.Fx.BuildingBlocks;
using Backend.Fx.ConfigurationSettings;
using Backend.Fx.Patterns.IdGeneration;
using JetBrains.Annotations;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.ConfigurationSettings
{
    public class TheSetting : TestWithLogging
    {
        [UsedImplicitly]
        public class TestSettingsService : SettingsService
        {
            public TestSettingsService(IEntityIdGenerator idGenerator, IRepository<Setting> settingRepository)
                : base("Test", idGenerator, settingRepository, new SettingSerializerFactory())
            {
            }
        }

        [Fact]
        public void CanStoreBoolean()
        {
            const bool booleanValue = true;
            var sut = new Setting(3, "key");
            sut.SetValue(new BooleanSerializer(), booleanValue);
            Assert.Equal("True", sut.SerializedValue);
            var booleanValueRead = sut.GetValue(new BooleanSerializer());
            Assert.Equal(booleanValue, booleanValueRead);
        }

        [Fact]
        public void CanStoreDateTime()
        {
            var dateTimeValue = new DateTime(1987, 4, 22, 23, 12, 11);
            var sut = new Setting(9, "key");
            sut.SetValue(new DateTimeSerializer(), dateTimeValue);
            Assert.Equal("1987-04-22T23:12:11.0000000", sut.SerializedValue);
            var dateTimeValueRead = sut.GetValue(new DateTimeSerializer());
            Assert.Equal(dateTimeValue, dateTimeValueRead);
        }

        [Fact]
        public void CanStoreDouble()
        {
            const double doubleValue = 2354.2341234d;
            var sut = new Setting(5, "key");
            sut.SetValue(new DoubleSerializer(), doubleValue);
            Assert.Equal("2354.2341234", sut.SerializedValue);
            var doubleValueRead = sut.GetValue(new DoubleSerializer());
            Assert.Equal(doubleValue, doubleValueRead);
        }

        [Fact]
        public void CanStoreInt()
        {
            const int intValue = 235234;
            var sut = new Setting(7, "key");
            sut.SetValue(new IntegerSerializer(), intValue);
            Assert.Equal("235234", sut.SerializedValue);
            var intValueRead = sut.GetValue(new IntegerSerializer());
            Assert.Equal(intValue, intValueRead);
        }

        [Fact]
        public void CanStoreNullBoolean()
        {
            var sut = new Setting(4, "key");
            sut.SetValue(new BooleanSerializer(), null);
            Assert.Null(sut.SerializedValue);
            var booleanValueRead = sut.GetValue(new BooleanSerializer());
            Assert.Null(booleanValueRead);
        }

        [Fact]
        public void CanStoreNullDateTime()
        {
            var sut = new Setting(10, "key");
            sut.SetValue(new DateTimeSerializer(), null);
            Assert.Null(sut.SerializedValue);
            var dateTimeValueRead = sut.GetValue(new DateTimeSerializer());
            Assert.Null(dateTimeValueRead);
        }

        [Fact]
        public void CanStoreNullDouble()
        {
            var sut = new Setting(6, "key");
            sut.SetValue(new DoubleSerializer(), null);
            Assert.Null(sut.SerializedValue);
            var doubleValueRead = sut.GetValue(new DoubleSerializer());
            Assert.Null(doubleValueRead);
        }

        [Fact]
        public void CanStoreNullInt()
        {
            var sut = new Setting(8, "key");
            sut.SetValue(new IntegerSerializer(), null);
            Assert.Null(sut.SerializedValue);
            var intValueRead = sut.GetValue(new IntegerSerializer());
            Assert.Null(intValueRead);
        }

        [Fact]
        public void CanStoreNullString()
        {
            const string stringValue = null;
            var sut = new Setting(2, "key");
            sut.SetValue(new StringSerializer(), stringValue);
            Assert.Equal(stringValue, sut.SerializedValue);
            var stringValueRead = sut.GetValue(new StringSerializer());
            Assert.Equal(stringValue, stringValueRead);
        }

        [Fact]
        public void CanStoreString()
        {
            const string stringValue = "sdufhpsdfb ^ ÄÜÖÄÜ psdj";
            var sut = new Setting(1, "key");
            sut.SetValue(new StringSerializer(), stringValue);
            Assert.Equal(stringValue, sut.SerializedValue);
            var stringValueRead = sut.GetValue(new StringSerializer());
            Assert.Equal(stringValue, stringValueRead);
        }

        public TheSetting(ITestOutputHelper output) : base(output)
        {
        }
    }
}