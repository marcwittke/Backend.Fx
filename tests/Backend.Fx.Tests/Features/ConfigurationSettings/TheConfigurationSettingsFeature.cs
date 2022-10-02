using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Backend.Fx.Features.ConfigurationSettings;
using Backend.Fx.Features.ConfigurationSettings.InMem;
using Backend.Fx.Logging;
using Backend.Fx.MicrosoftDependencyInjection;
using Backend.Fx.TestUtil;
using FakeItEasy;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Features.ConfigurationSettings;

public class TheConfigurationSettingsFeature : TestWithLogging
{
    private readonly IBackendFxApplication _sut;
    private readonly IExceptionLogger _exceptionLogger = A.Fake<IExceptionLogger>();

    public TheConfigurationSettingsFeature(ITestOutputHelper output) : base(output)
    {
        _sut = new BackendFxApplication(new MicrosoftCompositionRoot(), _exceptionLogger, GetType().Assembly);
        _sut.EnableFeature(new ConfigurationSettingsFeature<TestSettingsRepository>());
    }

    [Fact]
    public async Task HasInjectedSettingsSerializerFactory()
    {
        await _sut.BootAsync();
        var settingSerializerFactory =
            _sut.CompositionRoot.ServiceProvider.GetRequiredService<ISettingSerializerFactory>();
        Assert.IsType<SettingSerializerFactory>(settingSerializerFactory);
    }

    [Fact]
    public async Task CanWriteAndReadSettings()
    {
        await _sut.BootAsync();
        await _sut.Invoker.InvokeAsync(sp =>
        {
            var category = sp.GetRequiredService<TestSettingsCategory>();
            category.MyIntegerSetting = 42;
            category.MyDoubleSetting = 42.42d;
            category.MyBooleanSetting = true;
            category.MyStringSetting = "Forty two";
            category.MyLocalDateTimeSetting = new LocalDateTime(2000, 1, 2, 3, 4, 5);
            category.MyDateTimeSetting = new DateTime(2001, 1, 2, 3, 4, 5);
            return Task.CompletedTask;
        }, new ClaimsIdentity());
        
        Assert.Equal("42",TestSettingsRepository.DummyStore["my"]["MyIntegerSetting"]);
        Assert.Equal("42.42",TestSettingsRepository.DummyStore["my"]["MyDoubleSetting"]);
        Assert.Equal("True",TestSettingsRepository.DummyStore["my"]["MyBooleanSetting"]);
        Assert.Equal("Forty two",TestSettingsRepository.DummyStore["my"]["MyStringSetting"]);
        Assert.Equal("2000-01-02T03:04:05.0000000",TestSettingsRepository.DummyStore["my"]["MyLocalDateTimeSetting"]);
        Assert.Equal("2001-01-02T03:04:05.0000000",TestSettingsRepository.DummyStore["my"]["MyDateTimeSetting"]);
        
        await _sut.Invoker.InvokeAsync(sp =>
        {
            var category = sp.GetRequiredService<TestSettingsCategory>();
            Assert.Equal(42, category.MyIntegerSetting);
            Assert.Equal(42.42d, category.MyDoubleSetting);
            Assert.True(category.MyBooleanSetting);
            Assert.Equal("Forty two", category.MyStringSetting);
            Assert.Equal(new LocalDateTime(2000, 1, 2, 3, 4, 5), category.MyLocalDateTimeSetting);
            Assert.Equal(new DateTime(2001, 1, 2, 3, 4, 5), category.MyDateTimeSetting);
            return Task.CompletedTask;
        }, new ClaimsIdentity());
    }


    [UsedImplicitly]
    private class TestSettingsRepository : InMemorySettingRepository
    {
        public static readonly ConcurrentDictionary<string, Dictionary<string, string>> DummyStore = new();

        protected override ConcurrentDictionary<string, Dictionary<string, string>> SettingsStore => DummyStore;
    }

    [UsedImplicitly]
    public class TestSettingsCategory : SettingsCategory
    {
        public TestSettingsCategory(ISettingRepository settingRepository, ISettingSerializerFactory settingSerializerFactory) 
            : base("my", settingRepository, settingSerializerFactory)
        {
        }
        
        public int MyIntegerSetting
        {
             get => ReadSetting<int?>(nameof(MyIntegerSetting)) ?? 0;
             set => WriteSetting<int?>(nameof(MyIntegerSetting), value);
         }
        
        public bool MyBooleanSetting
        {
            get => ReadSetting<bool?>(nameof(MyBooleanSetting)) ?? false;
            set => WriteSetting<bool?>(nameof(MyBooleanSetting), value);
        }
        
        public double MyDoubleSetting
        {
            get => ReadSetting<double?>(nameof(MyDoubleSetting)) ?? 0d;
            set => WriteSetting<double?>(nameof(MyDoubleSetting), value);
        }
        
        public LocalDateTime? MyLocalDateTimeSetting
        {
            get => ReadSetting<LocalDateTime?>(nameof(MyLocalDateTimeSetting)) ?? null;
            set => WriteSetting(nameof(MyLocalDateTimeSetting), value);
        }
        
        public DateTime? MyDateTimeSetting
        {
            get => ReadSetting<DateTime?>(nameof(MyDateTimeSetting)) ?? null;
            set => WriteSetting(nameof(MyDateTimeSetting), value);
        }
        
        public string MyStringSetting
        {
            get => ReadSetting<string>(nameof(MyStringSetting));
            set => WriteSetting(nameof(MyStringSetting), value);
        }
    }
}