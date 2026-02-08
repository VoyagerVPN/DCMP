using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using dc;
using ModCore.Utilities;

namespace DCMP.Utils;

public static class LocalizationService
{
    private static readonly Dictionary<string, string> _cachedStrings = new();
    private static string _currentLang = "en";
    private static string _modDir = "";
    private const string SENTINEL = "dcmp_injected";
    private static bool _initialized = false;

    public static string CurrentLanguage => _currentLang;

    public static void Initialize(string modDir)
    {
        _modDir = modDir ?? "";
        _initialized = true;
        
        // Initial load (likely with fallback to en)
        UpdateLanguageAndLoad();
    }

    private static void UpdateLanguageAndLoad()
    {
        string? gameLang = null;
        try 
        {
            // Try different ways to get language id
            gameLang = Lang.Class.LANG?.ToString();
            
            // In some versions LANG might be a complex object or not set yet
            if (string.IsNullOrEmpty(gameLang) || gameLang == "??")
            {
                var current = Lang.Class.getCurrent();
                if (current != null && current.id != null)
                {
                    gameLang = current.id.ToString();
                }
            }
        }
        catch {}

        string detected = (gameLang ?? "en").ToLower();
        if (detected == "??" ) detected = "en";

        // Only reload if language changed or first run
        if (_cachedStrings.Count == 0 || _currentLang != detected)
        {
            _currentLang = detected;
            LoadStrings(_currentLang);
        }
    }

    private static void LoadStrings(string langCode)
    {
        if (string.IsNullOrEmpty(_modDir)) return;

        string langPath = Path.Combine(_modDir, "Resources", "lang", $"{langCode}.json");

        // Fallback to English if file doesn't exist
        if (!File.Exists(langPath))
        {
            Logger.Warning($"[Localization] No strings for {langCode}, falling back to English. Checked: {langPath}");
            langPath = Path.Combine(_modDir, "Resources", "lang", "en.json");
        }

        if (File.Exists(langPath))
        {
            try 
            {
                string json = File.ReadAllText(langPath);
                var data = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                if (data != null)
                {
                    _cachedStrings.Clear();
                    foreach (var kvp in data)
                    {
                        _cachedStrings[kvp.Key] = kvp.Value;
                    }
                    // Marker for injection
                    _cachedStrings[SENTINEL] = "true";
                    Logger.Information($"[Localization] Loaded {_cachedStrings.Count - 1} strings for language: {langCode}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"[Localization] Failed to parse JSON at {langPath}: {ex.Message}");
            }
        }
    }

    private static void InjectIntoGame()
    {
        // 1. Check if game language changed since last load
        UpdateLanguageAndLoad();

        if (_cachedStrings.Count == 0) return;

        var t = Lang.Class.t;
        if (t == null) return;

        var gameTexts = t.texts;
        if (gameTexts == null) return;

        // 2. Check if already injected into THIS specific instance of texts
        if (gameTexts.exists(SENTINEL.AsHaxeString()))
        {
            return;
        }

        foreach (var kvp in _cachedStrings)
        {
            gameTexts.set(kvp.Key.AsHaxeString(), kvp.Value.AsHaxeString());
        }
        
        Logger.Information($"[Localization] Injected {_cachedStrings.Count} strings into game dictionary (Lang: {_currentLang})");
    }

    public static dc.String Translate(string key, object? args = null)
    {
        if (!_initialized) return key.AsHaxeString();

        InjectIntoGame();

        string result = "";
        try 
        {
            if (Lang.Class.t != null)
            {
                var translated = Lang.Class.t.get(key.AsHaxeString(), args);
                if (translated != null && translated.ToString() != key)
                {
                    return translated;
                }
            }
        }
        catch { }

        // Fallback
        if (_cachedStrings.TryGetValue(key, out string? value))
        {
            result = value;
        }
        else
        {
            result = key;
        }

        if (args != null && result.Contains("::"))
        {
            foreach (var prop in args.GetType().GetProperties())
            {
                string pKey = $"::{prop.Name}::";
                string pVal = prop.GetValue(args)?.ToString() ?? "";
                result = result.Replace(pKey, pVal);
            }
        }
        
        return result.AsHaxeString();
    }
}
