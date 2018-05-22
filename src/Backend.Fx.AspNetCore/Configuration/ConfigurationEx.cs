namespace Backend.Fx.AspNetCore.Configuration
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;

    public static class ConfigurationEx
    {
        public static TOptions Load<TOptions>(this IConfiguration configuration) where TOptions : class, new()
        {
            var configurationSection = configuration.GetSection(typeof(TOptions).Name);
            var configurationOptions = new NamedConfigureFromConfigurationOptions<TOptions>(
                    typeof(TOptions).Name,
                    configurationSection);
            var options = new TOptions();
            configurationOptions.Action(options);
            return options;
        }
    }
}