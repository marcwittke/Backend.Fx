namespace Backend.Fx.Tests.ConfigurationSettings
{
    using System;
    using Fx.BuildingBlocks;
    using Fx.ConfigurationSettings;
    using NLogLogging;
    using Xunit;

    public class TheSetting : IClassFixture<NLogLoggingFixture>
    {
        public class TestSettingsService : SettingsService
        {
            public TestSettingsService(IRepository<Setting> settingRepository) : base(settingRepository)
            {}
        }

        [Fact]
        public void CanStoreString()
        {
            const string stringValue = "sdufhpsdfb ^ ÄÜÖÄÜ psdj";
            Setting sut = new Setting("key");
            sut.SetValue(new StringSerializer(), stringValue);
            Assert.Equal(stringValue, sut.SerializedValue);
            var stringValueRead = sut.GetValue(new StringSerializer());
            Assert.Equal(stringValue, stringValueRead);
        }

        [Fact]
        public void CanStoreNullString()
        {
            const string stringValue = null;
            Setting sut = new Setting("key");
            sut.SetValue(new StringSerializer(), stringValue);
            Assert.Equal(stringValue, sut.SerializedValue);
            var stringValueRead = sut.GetValue(new StringSerializer());
            Assert.Equal(stringValue, stringValueRead);
        }

        [Fact]
        public void CanStoreBoolean()
        {
            const bool booleanValue = true;
            Setting sut = new Setting("key");
            sut.SetValue(new BooleanSerializer(), booleanValue);
            Assert.Equal("True", sut.SerializedValue);
            var booleanValueRead = sut.GetValue(new BooleanSerializer());
            Assert.Equal(booleanValue, booleanValueRead);
        }

        [Fact]
        public void CanStoreNullBoolean()
        {
            Setting sut = new Setting("key");
            sut.SetValue(new BooleanSerializer(), null);
            Assert.Equal(null, sut.SerializedValue);
            var booleanValueRead = sut.GetValue(new BooleanSerializer());
            Assert.Equal(null, booleanValueRead);
        }

        [Fact]
        public void CanStoreDouble()
        {
            const double doubleValue = 2354.2341234d;
            Setting sut = new Setting("key");
            sut.SetValue(new DoubleSerializer(), doubleValue);
            Assert.Equal("2354.2341234", sut.SerializedValue);
            var doubleeanValueRead = sut.GetValue(new DoubleSerializer());
            Assert.Equal(doubleValue, doubleeanValueRead);
        }

        [Fact]
        public void CanStoreNullDouble()
        {
            Setting sut = new Setting("key");
            sut.SetValue(new DoubleSerializer(), null);
            Assert.Equal(null, sut.SerializedValue);
            var doubleValueRead = sut.GetValue(new DoubleSerializer());
            Assert.Equal(null, doubleValueRead);
        }

        [Fact]
        public void CanStoreInt()
        {
            const int intValue = 235234;
            Setting sut = new Setting("key");
            sut.SetValue(new IntegerSerializer(), intValue);
            Assert.Equal("235234", sut.SerializedValue);
            var inteanValueRead = sut.GetValue(new IntegerSerializer());
            Assert.Equal(intValue, inteanValueRead);
        }

        [Fact]
        public void CanStoreNullInt()
        {
            Setting sut = new Setting("key");
            sut.SetValue(new IntegerSerializer(), null);
            Assert.Equal(null, sut.SerializedValue);
            var intValueRead = sut.GetValue(new IntegerSerializer());
            Assert.Equal(null, intValueRead);
        }

        [Fact]
        public void CanStoreDateTime()
        {
            DateTime dateTimeValue = new DateTime(1987, 4, 22, 23, 12, 11);
            Setting sut = new Setting("key");
            sut.SetValue(new DateTimeSerializer(), dateTimeValue);
            Assert.Equal("Wed, 22 Apr 1987 23:12:11 GMT", sut.SerializedValue);
            var dateTimeValueRead = sut.GetValue(new DateTimeSerializer());
            Assert.Equal(dateTimeValue, dateTimeValueRead);
        }

        [Fact]
        public void CanStoreNullDateTime()
        {
            Setting sut = new Setting("key");
            sut.SetValue(new DateTimeSerializer(), null);
            Assert.Equal(null, sut.SerializedValue);
            var dateTimeValueRead = sut.GetValue(new DateTimeSerializer());
            Assert.Equal(null, dateTimeValueRead);
        }
    }
}
