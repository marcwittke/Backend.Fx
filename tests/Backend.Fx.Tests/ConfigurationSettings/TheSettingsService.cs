﻿using System.Linq;
using Backend.Fx.Domain;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Features.Authorization;
using Backend.Fx.Features.ConfigurationSettings;
using Backend.Fx.Hacking;
using Backend.Fx.InMemoryPersistence;
using Backend.Fx.TestUtil;
using FakeItEasy;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.ConfigurationSettings
{
    public class TheSettingsService : TestWithLogging
    {
        public TheSettingsService(ITestOutputHelper output): base(output)
        {
            var settingAuthorization = A.Fake<IAuthorizationPolicy<Setting>>();
            A.CallTo(() => settingAuthorization.HasAccessExpression).Returns(setting => true);
            A.CallTo(() => settingAuthorization.Filter(A<IQueryable<Setting>>._)).ReturnsLazily((IQueryable<Setting> q) => q);
            A.CallTo(() => settingAuthorization.CanCreate(A<Setting>._)).Returns(true);

            _idGenerator = A.Fake<IEntityIdGenerator>();
            var nextId = 1;
            A.CallTo(() => _idGenerator.NextId()).ReturnsLazily(() => nextId++);
            _settingRepository = new InMemoryRepository<Setting>(new InMemoryStore<Setting>(), CurrentTenantIdHolder.Create(999), settingAuthorization);
        }

        public class MySettingsCategory : SettingsCategory
        {
            public MySettingsCategory(IEntityIdGenerator idGenerator, IRepository<Setting> repo)
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

        [Fact]
        public void ReadsNonExistingSettingAsDefaultFromRepository()
        {
            var sut = new MySettingsCategory(_idGenerator, _settingRepository);
            Assert.Null(sut.SmtpHost);
        }

        [Fact]
        public void ReadsNullSettingFromRepository()
        {
            var setting = new Setting(3, "My.SmtpHost");
            setting.SetPrivate(set => set.SerializedValue, null);

            _settingRepository.Add(setting);

            var sut = new MySettingsCategory(_idGenerator, _settingRepository);
            Assert.Null(sut.SmtpHost);
        }

        [Fact]
        public void ReadsSettingFromRepository()
        {
            var setting = new Setting(1, "My.SmtpPort");
            setting.SetPrivate(set => set.SerializedValue, "333");

            _settingRepository.Add(setting);

            var sut = new MySettingsCategory(_idGenerator, _settingRepository);
            Assert.Equal(333, sut.SmtpPort);
        }

        [Fact]
        public void StoresSettingsInRepository()
        {
            var sut = new MySettingsCategory(_idGenerator, _settingRepository) {SmtpPort = 333};
            Assert.Equal(333, sut.SmtpPort);

            var settings = _settingRepository.GetAll();
            Assert.Single(settings);
            Assert.Equal("333", settings[0].SerializedValue);
            Assert.Equal("My.SmtpPort", settings[0].Key);
        }
    }
}