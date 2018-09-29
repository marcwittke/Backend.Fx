using Backend.Fx.Hacking;
using Backend.Fx.InMemoryPersistence;
using Xunit;

namespace Backend.Fx.Tests.ConfigurationSettings
{
    using System.Linq;
    using FakeItEasy;
    using Fx.BuildingBlocks;
    using Fx.ConfigurationSettings;
    using Fx.Environment.MultiTenancy;
    using Fx.Patterns.Authorization;
    using Fx.Patterns.IdGeneration;
    using Xunit;

    public class TheSettingsService
    {
        public class MySettingsService : SettingsService
        {
            public MySettingsService(IEntityIdGenerator idGenerator, IRepository<Setting> repo) 
                : base("My", idGenerator, repo, new SettingSerializerFactory())
            {
            }
            public int SmtpPort
            {
                get => ReadSetting<int?>(nameof(SmtpPort)) ?? 25;
                set => WriteSetting<int?>(nameof(SmtpPort), value);
            }

            public string SmtpHost
            {
                get => ReadSetting<string>(nameof(SmtpHost));
                set => WriteSetting(nameof(SmtpHost), value);
            }
        }

        private readonly InMemoryRepository<Setting> _settingRepository;
        private readonly IEntityIdGenerator _idGenerator;

        public TheSettingsService()
        {
            var settingAuthorization = A.Fake<IAggregateAuthorization<Setting>>();
            A.CallTo(() => settingAuthorization.HasAccessExpression).Returns(setting => true);
            A.CallTo(() => settingAuthorization.Filter(A<IQueryable<Setting>>._)).ReturnsLazily((IQueryable<Setting> q) => q);
            A.CallTo(() => settingAuthorization.CanCreate(A<Setting>._)).Returns(true);

            _idGenerator = A.Fake<IEntityIdGenerator>();
            int nextId=1;
            A.CallTo(() => _idGenerator.NextId()).ReturnsLazily(() => nextId++);
            _settingRepository = new InMemoryRepository<Setting>(new InMemoryStore<Setting>(), CurrentTenantIdHolder.Create(999), settingAuthorization);
        }

        [Fact]
        public void StoresSettingsInRepository()
        {
            MySettingsService sut = new MySettingsService(_idGenerator, _settingRepository) {SmtpPort = 333};
            Assert.Equal(333, sut.SmtpPort);

            Setting[] settings = _settingRepository.GetAll();
            Assert.Single(settings);
            Assert.Equal("333", settings[0].SerializedValue);
            Assert.Equal("My.SmtpPort", settings[0].Key);
        }

        [Fact]
        public void ReadsSettingFromRepository()
        {
            var setting = new Setting(1, "My.SmtpPort");
            setting.SetPrivate(set => set.SerializedValue, "333");

            _settingRepository.Add(setting);

            MySettingsService sut = new MySettingsService(_idGenerator, _settingRepository);
            Assert.Equal(333, sut.SmtpPort);
        }

        [Fact]
        public void ReadsNullSettingFromRepository()
        {
            var setting = new Setting(3,"My.SmtpHost");
            setting.SetPrivate(set => set.SerializedValue, null);

            _settingRepository.Add(setting);

            MySettingsService sut = new MySettingsService(_idGenerator, _settingRepository);
            Assert.Null(sut.SmtpHost);
        }

        [Fact]
        public void ReadsNonExistingSettingAsDefaultFromRepository()
        {
            MySettingsService sut = new MySettingsService(_idGenerator, _settingRepository);
            Assert.Null(sut.SmtpHost);
        }
    }
}
