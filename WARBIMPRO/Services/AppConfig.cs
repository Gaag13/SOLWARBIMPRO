namespace WARBIMPRO.Services
{
    using System;
    using System.IO;
    using Newtonsoft.Json;

    public class AppConfig
    {
        public string ApiKey { get; set; } = string.Empty;
        public string LibraryPath { get; set; } = string.Empty;
    }

    public static class ConfigService
    {
        private static readonly string ConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WARBIMPRO", "claude_config.json");

        public static AppConfig Load()
        {
            if (!File.Exists(ConfigPath)) return new AppConfig();
            try
            {
                return JsonConvert.DeserializeObject<AppConfig>(File.ReadAllText(ConfigPath))
                      ?? new AppConfig();
            }
            catch { return new AppConfig(); }
        }

        public static void Save(AppConfig config)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath)!);
            File.WriteAllText(ConfigPath,
                JsonConvert.SerializeObject(config, Formatting.Indented));
        }
    }
}
