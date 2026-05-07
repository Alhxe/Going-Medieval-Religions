using HarmonyLib;
using I2.Loc;
using NSMedieval.Controllers;
using System.Collections.Generic;

namespace Alhxe.ReligionsExpanded.Patches
{
    /// <summary>
    /// Replaces specific I2 Localization values at lookup time. Going Medieval
    /// ships its two religions under generic-sounding names ("Restitucionista"
    /// / "Fiel del Roble") — we surface them as "Christian" / "Pagan" instead
    /// so future religions added by Religions Expanded slot in alongside.
    ///
    /// We postfix every translation entry-point (I2 + the game's wrapper) so
    /// it doesn't matter which one a given UI calls.
    /// </summary>
    internal static class LocalizationRename
    {
        // term -> { language -> override }
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
            ["religion_pagan_name"] = new Dictionary<string, string> {
                ["English"] = "Paganism",
                ["Spanish"] = "Paganismo",
            },
            ["resource_name_restitutionist_relic"] = new Dictionary<string, string> {
                ["English"] = "Holy Relic",
                ["Spanish"] = "Reliquia sagrada",
            },
        };

        internal static string Translate(string term, string fallback)
        {
            if (string.IsNullOrEmpty(term)) return fallback;
            if (!Overrides.TryGetValue(term, out var byLang)) return fallback;

            string lang = LocalizationManager.CurrentLanguage ?? "English";
            if (byLang.TryGetValue(lang, out var t)) return t;
            if (byLang.TryGetValue("English", out var en)) return en;
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
            if (!ReferenceEquals(newVal, Translation))
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
        [HarmonyPostfix]
        private static void Postfix(string key, ref string __result)
        {
            __result = LocalizationRename.Translate(key, __result);
        }
    }
}
