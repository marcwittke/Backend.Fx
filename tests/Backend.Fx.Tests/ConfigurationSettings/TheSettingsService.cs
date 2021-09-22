using System.Linq;
using Backend.Fx.BuildingBlocks;
using Backend.Fx.ConfigurationSettings;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Hacking;
using Backend.Fx.InMemoryPersistence;
using Backend.Fx.Patterns.Authorization;
using Backend.Fx.Patterns.IdGeneration;
using FakeItEasy;
using Xunit;

namespace Backend.Fx.Tests.ConfigurationSettings
{
    public class TheSettingsService
    {
        private readonly IEntityIdGenerator _idGenerator;

        private readonly InMemoryRepository<Setting> _settingRepository;

        public TheSettingsService()
        {
            var settingAuthorization = A.Fake<IAggregateAuthorization<Setting>>();
            A.CallTo(() => settingAuthorization.HasAccessExpression).Returns(setting => true);
            A.CallTo(() => settingAuthorization.Filter(A<IQueryable<Setting>>._))
                .ReturnsLazily((IQueryable<Setting> q) => q);
            A.CallTo(() => settingAuthorization.CanCreate(A<Setting>._)).Returns(true);

            _idGenerator = A.Fake<IEntityIdGenerator>();
            var nextId = 1;
            A.CallTo(() => _idGenerator.NextId()).ReturnsLazily(() => nextId++);
            _settingRepository = new InMemoryRepository<Setting>(
                new InMemoryStore<Setting>(),
                CurrentTenantIdHolder.Create(999),
                settingAuthorization);
        }

        [Fact]
        public void ReadsNonExistingSettingAsDefaultFromRepository()
        {
            var sut = new MySettingsService(_idGenerator, _settingRepository);
            Assert.Null(sut.SmtpHost);
        }

        [Fact]
        public void ReadsNullSettingFromRepository()
        {
            var setting = new Setting(3, "My.SmtpHost");
            setting.SetPrivate(set => set.SerializedValue, null);

            _settingRepository.Add(setting);

            var sut = new MySettingsService(_idGenerator, _settingRepository);
            Assert.Null(sut.SmtpHost);
        }

        [Fact]
        public void ReadsSettingFromRepository()
        {
            var setting = new Setting(1, "My.SmtpPort");
            setting.SetPrivate(set => set.SerializedValue, "333");

            _settingRepository.Add(setting);

            var sut = new MySettingsService(_idGenerator, _settingRepository);
            Assert.Equal(333, sut.SmtpPort);
        }

        [Fact]
        public void StoresSettingsInRepository()
        {
            var sut = new MySettingsService(_idGenerator, _settingRepository) { SmtpPort = 333 };
            Assert.Equal(333, sut.SmtpPort);

            Setting[] settings = _settingRepository.GetAll();
            Assert.Single(settings);
            Assert.Equal("333", settings[0].SerializedValue);
            Assert.Equal("My.SmtpPort", settings[0].Key);
        }


        public class MySettingsService : SettingsService
        {
            public MySettingsService(IEntityIdGenerator idGenerator, IRepository<Setting> repo)
                : base("My", idGenerator, repo, new SettingSerializerFactory())
            { }

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
    }
}
