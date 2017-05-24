namespace Backend.Fx.Tests.ConfigurationSettings
{
    using Fx.BuildingBlocks;
    using Fx.ConfigurationSettings;
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
        }
        
        [Fact]
        public void StoresSettingsInRepository()
        {
            var inMemoryRepository = new InMemoryRepository<Setting>();

            MySettingsService sut = new MySettingsService(inMemoryRepository);
            sut.SmtpPort = 333;
            Assert.Equal(333, sut.SmtpPort);

            Setting[] settings = inMemoryRepository.GetAll();
            Assert.Equal(1, settings.Length);
            Assert.Equal("333", settings[0].SerializedValue);
            Assert.Equal("SmtpPort", settings[0].Key);
        }

        [Fact]
        public void ReadsSettingFromRepository()
        {
            var setting = new Setting("SmtpPort");
            setting.SetPrivate(set => set.SerializedValue, "333");

            var inMemoryRepository = new InMemoryRepository<Setting>();
            inMemoryRepository.Add(setting);

            MySettingsService sut = new MySettingsService(inMemoryRepository);
            Assert.Equal(333, sut.SmtpPort);
        }
    }
}
