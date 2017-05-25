namespace Backend.Fx.Tests.ConfigurationSettings
{
    using FakeItEasy;
    using Fx.BuildingBlocks;
    using Fx.ConfigurationSettings;
    using Fx.Environment.MultiTenancy;
    using Fx.Patterns.Authorization;
    using Fx.Patterns.DependencyInjection;
    using Testing;
    using Xunit;

    public class TheSettingsService
    {
        public class MySettingsService : SettingsService
        {
            public MySettingsService(IRepository<Setting> repo) : base(repo)
            {
            }
            public int SmtpPort
            {
                get { return ReadSetting<int?>(nameof(SmtpPort)) ?? 25; }
                set { WriteSetting<int?>(nameof(SmtpPort), value); }
            }

            public string SmtpHost
            {
                get { return ReadSetting<string>(nameof(SmtpHost)); }
                set { WriteSetting<string>(nameof(SmtpHost), value); }
            }
        }

        private readonly IRepository<Setting> settingRepository;

        public TheSettingsService()
        {
            var tenantHolder = A.Fake<ICurrentTHolder<TenantId>>();
            A.CallTo(() => tenantHolder.Current).Returns(new TenantId(999));

            var settingAuthorization = A.Fake<IAggregateRootAuthorization<Setting>>();
            A.CallTo(() => settingAuthorization.HasAccessExpression).Returns(setting => true);
            A.CallTo(() => settingAuthorization.CanCreate()).Returns(true);
            settingRepository = new InMemoryRepository<Setting>(tenantHolder, settingAuthorization);
        }

        [Fact]
        public void StoresSettingsInRepository()
        {
            MySettingsService sut = new MySettingsService(settingRepository);
            sut.SmtpPort = 333;
            Assert.Equal(333, sut.SmtpPort);

            Setting[] settings = settingRepository.GetAll();
            Assert.Equal(1, settings.Length);
            Assert.Equal("333", settings[0].SerializedValue);
            Assert.Equal("SmtpPort", settings[0].Key);
        }

        [Fact]
        public void ReadsSettingFromRepository()
        {
            var setting = new Setting("SmtpPort");
            setting.SetPrivate(set => set.SerializedValue, "333");

            settingRepository.Add(setting);

            MySettingsService sut = new MySettingsService(settingRepository);
            Assert.Equal(333, sut.SmtpPort);
        }

        [Fact]
        public void ReadsNullSettingFromRepository()
        {
            var setting = new Setting("SmtpHost");
            setting.SetPrivate(set => set.SerializedValue, null);

            settingRepository.Add(setting);

            MySettingsService sut = new MySettingsService(settingRepository);
            Assert.Equal(null, sut.SmtpHost);
        }

        [Fact]
        public void ReadsNonExistingSettingAsDefaultFromRepository()
        {
            MySettingsService sut = new MySettingsService(settingRepository);
            Assert.Equal(null, sut.SmtpHost);
        }
    }
}
