namespace Backend.Fx.Tests.ConfigurationSettings
{
    using System.Linq;
    using FakeItEasy;
    using Fx.BuildingBlocks;
    using Fx.ConfigurationSettings;
    using Fx.Environment.MultiTenancy;
    using Fx.Patterns.Authorization;
    using Fx.Patterns.IdGeneration;
    using Testing;
    using Testing.InMemoryPersistence;
    using Xunit;

    public class TheSettingsService
    {
        public class MySettingsService : SettingsService
        {
            public MySettingsService(IEntityIdGenerator idGenerator, IRepository<Setting> repo) 
                : base(idGenerator, repo, new SettingSerializerFactory())
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

        private readonly InMemoryRepository<Setting> settingRepository;
        private readonly IEntityIdGenerator idGenerator;

        public TheSettingsService()
        {
            var settingAuthorization = A.Fake<IAggregateAuthorization<Setting>>();
            A.CallTo(() => settingAuthorization.HasAccessExpression).Returns(setting => true);
            A.CallTo(() => settingAuthorization.Filter(A<IQueryable<Setting>>._)).ReturnsLazily((IQueryable<Setting> q) => q);
            A.CallTo(() => settingAuthorization.CanCreate(A<Setting>._)).Returns(true);

            idGenerator = A.Fake<IEntityIdGenerator>();
            int nextId=1;
            A.CallTo(() => idGenerator.NextId()).ReturnsLazily(() => nextId++);
            settingRepository = new InMemoryRepository<Setting>(new InMemoryStore<Setting>(), new TenantId(999), settingAuthorization);
        }

        [Fact]
        public void StoresSettingsInRepository()
        {
            MySettingsService sut = new MySettingsService(idGenerator, settingRepository) {SmtpPort = 333};
            Assert.Equal(333, sut.SmtpPort);

            Setting[] settings = settingRepository.GetAll();
            Assert.Equal(1, settings.Length);
            Assert.Equal("333", settings[0].SerializedValue);
            Assert.Equal("SmtpPort", settings[0].Key);
        }

        [Fact]
        public void ReadsSettingFromRepository()
        {
            var setting = new Setting(1, "SmtpPort");
            setting.SetPrivate(set => set.SerializedValue, "333");

            settingRepository.Add(setting);

            MySettingsService sut = new MySettingsService(idGenerator, settingRepository);
            Assert.Equal(333, sut.SmtpPort);
        }

        [Fact]
        public void ReadsNullSettingFromRepository()
        {
            var setting = new Setting(3,"SmtpHost");
            setting.SetPrivate(set => set.SerializedValue, null);

            settingRepository.Add(setting);

            MySettingsService sut = new MySettingsService(idGenerator, settingRepository);
            Assert.Equal(null, sut.SmtpHost);
        }

        [Fact]
        public void ReadsNonExistingSettingAsDefaultFromRepository()
        {
            MySettingsService sut = new MySettingsService(idGenerator, settingRepository);
            Assert.Equal(null, sut.SmtpHost);
        }
    }
}
