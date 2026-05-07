using Alhxe.ReligionsExpanded.Helpers;
using HarmonyLib;
using I2.Loc;
using NSMedieval.Controllers;
using System.Collections.Generic;

namespace Alhxe.ReligionsExpanded.Patches
{
    /// <summary>
    /// Replaces I2 Localization lookups whenever the JSON mod's
    /// Localization/&lt;lang&gt;.json files contain a matching key. No hardcoded
    /// list — translators just drop a string into the locale file and it
    /// becomes an override. This makes adding religions / new strings painless.
    /// </summary>
    internal static class LocalizationRename
    {
        internal static string Translate(string term, string fallback)
        {
            if (string.IsNullOrEmpty(term)) return fallback;
            string overridden = LocaleLoader.Get(term, null);
            return overridden ?? fallback;
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
