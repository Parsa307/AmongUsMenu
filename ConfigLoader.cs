using System;
using System.IO;

namespace AmongUsMenu
{
    public static class ConfigLoader
    {
        private static readonly string ConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "AmongUsMenu", "config.txt");

        /// <summary>
        /// Save settings to the configuration file.
        /// </summary>
        /// <param name="unlockAllCosmetics">Whether to unlock all cosmetics.</param>
        public static void SaveSettings(bool unlockAllCosmetics)
        {
            var lines = new[]
            {
                $"UnlockAllCosmetics={unlockAllCosmetics}"
            };

            try
            {
                var directoryPath = Path.GetDirectoryName(ConfigPath);
                if (string.IsNullOrEmpty(directoryPath))
                {
                    throw new InvalidOperationException("Config directory path is invalid.");
                }

                Directory.CreateDirectory(directoryPath);
                File.WriteAllLines(ConfigPath, lines);
                MainMod.Logger.LogInfo("Config saved successfully.");
            }
            catch (Exception ex)
            {
                MainMod.Logger.LogError($"Failed to save config: {ex.Message}");
            }
        }

        /// <summary>
        /// Load settings from the configuration file.
        /// </summary>
        /// <returns>Configuration data with values for UnlockAllCosmetics.</returns>
        public static ConfigData LoadSettings()
        {
            var configData = new ConfigData();

            try
            {
                if (!File.Exists(ConfigPath))
                {
                    MainMod.Logger.LogInfo("Config file not found. Creating default config.");
                    SaveSettings(false);
                    return configData;
                }

                foreach (var line in File.ReadAllLines(ConfigPath))
                {
                    var parts = line.Split('=');
                    if (parts.Length == 2)
                    {
                        switch (parts[0])
                        {
                            case "UnlockAllCosmetics":
                                if (bool.TryParse(parts[1], out var cosmetics))
                                    configData.UnlockAllCosmetics = cosmetics;
                                break;
                        }
                    }
                }

                MainMod.Logger.LogInfo("Config loaded successfully.");
            }
            catch (Exception ex)
            {
                MainMod.Logger.LogError($"Failed to load config: {ex.Message}");
            }

            return configData;
        }
    }

    /// <summary>
    /// Class representing the configuration data for the mod.
    /// </summary>
    public class ConfigData
    {
        public bool UnlockAllCosmetics { get; set; } = false;
    }
}
