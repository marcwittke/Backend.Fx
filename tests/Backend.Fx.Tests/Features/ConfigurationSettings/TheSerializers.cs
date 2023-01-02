using System;
using System.Globalization;
using Backend.Fx.Features.ConfigurationSettings.Serializers;
using NodaTime;
using Xunit;

namespace Backend.Fx.Tests.Features.ConfigurationSettings;

public class TheSerializers
{
    [Fact]
    public void CanSerializeAnnualDate()
    {
        var data = new AnnualDate(10,20);
        var sut = new AnnualDateSerializer();
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de");
        var serialized = sut.Serialize(data);
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en");
        Assert.Equal(data, sut.Deserialize(serialized));
    }
    
    [Fact]
    public void CanSerializeBoolean()
    {
        var data = true;
        var sut = new BooleanSerializer();
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de");
        var serialized = sut.Serialize(data);
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en");
        Assert.Equal(data, sut.Deserialize(serialized));
    }
    
    [Fact]
    public void CanSerializeDateTimeOffset()
    {
        var data = DateTimeOffset.UtcNow;
        var sut = new DateTimeOffsetSerializer();
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de");
        var serialized = sut.Serialize(data);
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en");
        Assert.Equal(data, sut.Deserialize(serialized));
    }
    
    [Fact]
    public void CanSerializeDateTime()
    {
        var data = DateTime.UtcNow;
        var sut = new DateTimeSerializer();
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de");
        var serialized = sut.Serialize(data);
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en");
        Assert.Equal(data, sut.Deserialize(serialized));
    }
    
    [Fact]
    public void CanSerializeDecimal()
    {
        var data = 123.456m;
        var sut = new DecimalSerializer();
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de");
        var serialized = sut.Serialize(data);
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en");
        Assert.Equal(data, sut.Deserialize(serialized));
    }
    
    [Fact]
    public void CanSerializeDouble()
    {
        double data = 345.678;
        var sut = new DoubleSerializer();
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de");
        var serialized = sut.Serialize(data);
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en");
        Assert.Equal(data, sut.Deserialize(serialized));
    }
    
    [Fact]
    public void CanSerializeDuration()
    {
        var data = Duration.FromMinutes(123456);
        var sut = new DurationSerializer();
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de");
        var serialized = sut.Serialize(data);
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en");
        Assert.Equal(data, sut.Deserialize(serialized));
    }
    
    [Fact]
    public void CanSerializeFloat()
    {
        var data = 234.567f;
        var sut = new FloatSerializer();
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de");
        var serialized = sut.Serialize(data);
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en");
        Assert.Equal(data, sut.Deserialize(serialized));
    }
    
    [Fact]
    public void CanSerializeInstant()
    {
        var data = SystemClock.Instance.GetCurrentInstant();
        var sut = new InstantSerializer();
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de");
        var serialized = sut.Serialize(data);
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en");
        Assert.Equal(data, sut.Deserialize(serialized));
    }
    
    [Fact]
    public void CanSerializeInteger()
    {
        var data = 456787;
        var sut = new IntegerSerializer();
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de");
        var serialized = sut.Serialize(data);
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en");
        Assert.Equal(data, sut.Deserialize(serialized));
    }
    
    [Fact]
    public void CanSerializeLocalDate()
    {
        var data = new LocalDate(2022,10,20);
        var sut = new LocalDateSerializer();
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de");
        var serialized = sut.Serialize(data);
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en");
        Assert.Equal(data, sut.Deserialize(serialized));
    }
    
    [Fact]
    public void CanSerializeLocalDateTime()
    {
        var data = new LocalDateTime(2001,12,22,12,34,55,456);
        var sut = new LocalDateTimeSerializer();
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de");
        var serialized = sut.Serialize(data);
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en");
        Assert.Equal(data, sut.Deserialize(serialized));
    }
    
    [Fact]
    public void CanSerializeLocalTime()
    {
        var data = new LocalTime(10,20, 33);
        var sut = new LocalTimeSerializer();
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de");
        var serialized = sut.Serialize(data);
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en");
        Assert.Equal(data, sut.Deserialize(serialized));
    }
    
    [Fact]
    public void CanSerializeLong()
    {
        var data = 325234562364345L;
        var sut = new LongSerializer();
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de");
        var serialized = sut.Serialize(data);
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en");
        Assert.Equal(data, sut.Deserialize(serialized));
    }
    
    [Fact]
    public void CanSerializeOffsetDate()
    {
        var data = new OffsetDate(new LocalDate(2000,3,4), Offset.FromHours(-3));
        var sut = new OffsetDateSerializer();
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de");
        var serialized = sut.Serialize(data);
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en");
        Assert.Equal(data, sut.Deserialize(serialized));
    }
    
    [Fact]
    public void CanSerializeOffset()
    {
        var data = Offset.FromHoursAndMinutes(1,30);
        var sut = new OffsetSerializer();
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de");
        var serialized = sut.Serialize(data);
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en");
        Assert.Equal(data, sut.Deserialize(serialized));
    }
    
    [Fact]
    public void CanSerializeOffsetTime()
    {
        var data = new OffsetTime(new LocalTime(10,33), Offset.FromHours(3));
        var sut = new OffsetTimeSerializer();
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de");
        var serialized = sut.Serialize(data);
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en");
        Assert.Equal(data, sut.Deserialize(serialized));
    }
    
    [Fact]
    public void CanSerializeShort()
    {
        short data = 333;
        var sut = new ShortSerializer();
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de");
        var serialized = sut.Serialize(data);
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en");
        Assert.Equal(data, sut.Deserialize(serialized));
    }
    
    [Fact]
    public void CanSerializeTimeSpan()
    {
        var data = TimeSpan.FromMilliseconds(123235);
        var sut = new TimeSpanSerializer();
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de");
        var serialized = sut.Serialize(data);
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en");
        Assert.Equal(data, sut.Deserialize(serialized));
    }
}