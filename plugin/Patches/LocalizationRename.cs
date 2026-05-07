using HarmonyLib;
using I2.Loc;
using NSMedieval.Controllers;
using System.Collections.Generic;

namespace Alhxe.ChristianityExpanded.Patches
{
    internal static class LocalizationRename
    {
        // Real I2 keys discovered by reading ScenarioView source: `general_christian`,
        // `general_pagan`, plus the room/research keys we added in JSON.
        private static readonly Dictionary<string, Dictionary<string, string>> Overrides = new Dictionary<string, Dictionary<string, string>>
        {
            ["general_christian"] = new Dictionary<string, string> {
                ["English"] = "Christian",
                ["Spanish"] = "Cristiano",
            },
            ["general_pagan"] = new Dictionary<string, string> {
                ["English"] = "Pagan",
                ["Spanish"] = "Pagano",
            },
            ["religion_christian_name"] = new Dictionary<string, string> {
                ["English"] = "Christianity",
                ["Spanish"] = "Cristianismo",
            },
            ["resource_name_restitutionist_relic"] = new Dictionary<string, string> {
                ["English"] = "Holy Relic",
                ["Spanish"] = "Reliquia sagrada",
            },
        };

        private static readonly HashSet<string> _seen = new HashSet<string>();
        private static int _logged;

        internal static string Translate(string term, string fallback)
        {
            if (string.IsNullOrEmpty(term)) return fallback;

            if (fallback != null && _logged < 800 && _seen.Add(term))
            {
                _logged++;
                Plugin.Log?.LogInfo($"[loc] {term} -> {fallback}");
                try
                {
                    string path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "ce-loc-debug.txt");
                    System.IO.File.AppendAllText(path, term + "\t" + fallback + "\n");
                }
                catch { }
            }

            if (Overrides.TryGetValue(term, out var byLang))
            {
                string lang = LocalizationManager.CurrentLanguage ?? "English";
                if (byLang.TryGetValue(lang, out var t)) return t;
                if (byLang.TryGetValue("English", out var en)) return en;
            }
            return fallback;
        }
    }

    [HarmonyPatch(typeof(LocalizationManager), nameof(LocalizationManager.TryGetTranslation))]
    internal static class Patch_TryGetTranslation
    {
        [HarmonyPostfix]
        private static void Postfix(string Term, ref string Translation, ref bool __result)
        {
            string newVal = LocalizationRename.Translate(Term, Translation);
            if (newVal != Translation)
            {
                Translation = newVal;
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(LocalizationManager), nameof(LocalizationManager.GetTranslation))]
    internal static class Patch_GetTranslation
    {
        [HarmonyPostfix]
        private static void Postfix(string Term, ref string __result)
        {
            __result = LocalizationRename.Translate(Term, __result);
        }
    }

    [HarmonyPatch(typeof(LocalizationController), nameof(LocalizationController.GetText), new[] { typeof(string) })]
    internal static class Patch_LocalizationController_GetText
    {
        // Sentinel: write file once. If file appears, patch runs.
        private static int _hits;

        [HarmonyPrefix]
        private static void Prefix(string key)
        {
            if (System.Threading.Interlocked.Increment(ref _hits) <= 5)
            {
                try
                {
                    string p = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "ce-loc-debug.txt");
                    System.IO.File.AppendAllText(p, $"[CTRL-Prefix #{_hits}] {key}\n");
                }
                catch { }
            }
        }

        [HarmonyPostfix]
        private static void Postfix(string key, ref string __result)
        {
            __result = LocalizationRename.Translate(key, __result);
        }
    }
}
