using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace Alhxe.ReligionsExpanded.Helpers
{
    /// <summary>
    /// Reads the JSON mod's per-language string tables once at boot so the
    /// plugin and the JSON content share a single localization source.
    /// Translators only need to edit the same files contributors edit for the
    /// JSON content -- no C# rebuild required.
    ///
    /// File shape (one per language):
    ///   ModBuild/ReligionsExpanded/Localization/en.json
    ///   {
    ///     "language": "English",
    ///     "strings": { "key": "value", ... }
    ///   }
    /// </summary>
    internal static class LocaleLoader
    {
        // term -> { language -> translation }
        public static Dictionary<string, Dictionary<string, string>> Strings { get; private set; }
            = new Dictionary<string, Dictionary<string, string>>();

        public static void LoadFromMod(string modName = "ReligionsExpanded")
        {
            Strings = new Dictionary<string, Dictionary<string, string>>();

            string root = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Foxy Voxel", "Going Medieval", "Mods", modName, "Localization");

            if (!Directory.Exists(root))
            {
                Plugin.Log?.LogWarning($"[Locale] folder not found: {root}");
                return;
            }

            int filesLoaded = 0, termsLoaded = 0;
            foreach (var file in Directory.GetFiles(root, "*.json"))
            {
                try
                {
                    var data = JObject.Parse(File.ReadAllText(file));
                    string language = data.Value<string>("language");
                    if (string.IsNullOrEmpty(language)) continue;

                    var strings = data["strings"] as JObject;
                    if (strings == null) continue;

                    foreach (var pair in strings)
                    {
                        if (pair.Key.StartsWith("_")) continue;
                        if (pair.Value == null) continue;

                        if (!Strings.TryGetValue(pair.Key, out var byLang))
                        {
                            byLang = new Dictionary<string, string>();
                            Strings[pair.Key] = byLang;
                        }
                        byLang[language] = pair.Value.ToString();
                        termsLoaded++;
                    }
                    filesLoaded++;
                }
                catch (Exception ex)
                {
                    Plugin.Log?.LogError($"[Locale] failed to read {Path.GetFileName(file)}: {ex.Message}");
                }
            }

            Plugin.Log?.LogInfo($"[Locale] loaded {filesLoaded} file(s), {termsLoaded} string(s) total.");
        }

        /// <summary>Resolve a key in the current language; returns fallback if missing.</summary>
        public static string Get(string key, string fallback = null)
        {
            if (string.IsNullOrEmpty(key)) return fallback;
            if (!Strings.TryGetValue(key, out var byLang)) return fallback;

            string lang = I2.Loc.LocalizationManager.CurrentLanguage ?? "English";
            if (byLang.TryGetValue(lang, out var t)) return t;
            if (byLang.TryGetValue("English", out var en)) return en;
            return fallback;
        }
    }
}
