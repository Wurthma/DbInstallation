using Microsoft.Extensions.Configuration;
using System.IO;

namespace DbInstallation.Util
{
    public static class Common
    {
        public static string GetAppSetting(string settingName)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            IConfigurationSection configurationSection = configuration.GetSection("Settings").GetSection(settingName);
            return configurationSection.Value;
        }
    }
}
