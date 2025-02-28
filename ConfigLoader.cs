﻿namespace AmongUsMenu
{
    public static class ConfigLoader
    {
        private static readonly string ConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "AmongUsMenu", "config.txt");

        public static void SaveSettings(ConfigData config)
        {
            var lines = new[]
            {
                $"UnlockAllCosmetics={config.UnlockAllCosmetics}",
                $"NoClip={config.NoClip}",
                $"AntiBan={config.AntiBan}",
                $"UseVents={config.UseVents}",
                $"CopyChatMessages={config.CopyChatMessages}"
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

        public static ConfigData LoadSettings()
        {
            var configData = new ConfigData();

            try
            {
                if (!File.Exists(ConfigPath))
                {
                    SaveSettings(configData);
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
                                configData.UnlockAllCosmetics = bool.TryParse(parts[1], out var cosmetics) && cosmetics;
                                break;
                            case "NoClip":
                                configData.NoClip = bool.TryParse(parts[1], out var noclip) && noclip;
                                break;
                            case "AntiBan":
                                configData.AntiBan = bool.TryParse(parts[1], out var ban) && ban;
                                break;
                            case "UseVents":
                                configData.UseVents = bool.TryParse(parts[1], out var vents) && vents;
                                break;
                            case "CopyChatMessages":
                                configData.CopyChatMessages = bool.TryParse(parts[1], out var copy) && copy;
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

    public class ConfigData
    {
        public bool UnlockAllCosmetics { get; set; } = false;
        public bool NoClip { get; set; } = false;
        public bool AntiBan { get; set; } = false;
        public bool UseVents { get; set; } = false;
        public bool CopyChatMessages { get; set; } = false;
    }
}
