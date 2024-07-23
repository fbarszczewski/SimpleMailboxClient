using Microsoft.Extensions.Configuration;
using SimpleMailboxClient.Entities;

namespace SimpleMailboxClient.Utilities;

public class SettingsLoader
{
    private static readonly string ConfigDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Config");
    private static readonly string ConfigFile = Path.Combine(ConfigDirectory, "mailSettings.json");

    public static EmailConfig GetEmailConfig()
    {
        if (!File.Exists(ConfigFile)) throw new FileNotFoundException("Configuration file not found", ConfigFile);

        var configuration = new ConfigurationBuilder()
            .SetBasePath(ConfigDirectory)
            .AddJsonFile("mailSettings.json", false, true)
            .Build();


        return configuration.GetSection("EmailConfig").Get<EmailConfig>();
    }
}