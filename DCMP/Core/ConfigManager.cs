using System.IO;
using Newtonsoft.Json;
using DCMP.Utils;

namespace DCMP.Core;

public class ConfigManager
{
    private static string? _configDir;
    private static string ConfigPath => Path.Combine(_configDir ?? ".", "dcmp_config.json");

    public static ModConfig Current { get; private set; } = new();

    public static void Initialize(string? directory)
    {
        _configDir = directory;
        Load();
    }

    public static void Load()
    {
        try
        {
            if (File.Exists(ConfigPath))
            {
                string json = File.ReadAllText(ConfigPath);
                Current = JsonConvert.DeserializeObject<ModConfig>(json) ?? new ModConfig();
                Logger.Information("[Config] Configuration loaded.");
            }
            else
            {
                Save(); // Create default
            }
        }
        catch (System.Exception ex)
        {
            Logger.Error($"[Config] Failed to load config: {ex.Message}");
            Current = new ModConfig();
        }
    }

    public static void Save()
    {
        if (string.IsNullOrEmpty(_configDir)) return;

        try
        {
            string json = JsonConvert.SerializeObject(Current, Formatting.Indented);
            File.WriteAllText(ConfigPath, json);
            Logger.Information("[Config] Configuration saved.");
        }
        catch (System.Exception ex)
        {
            Logger.Error($"[Config] Failed to save config: {ex.Message}");
        }
    }
}

public class ModConfig
{
    public string LastServerIP { get; set; } = "127.0.0.1";
    public int ServerPort { get; set; } = 14600;
    public string PlayerName { get; set; } = "GuestPlayer";
}
