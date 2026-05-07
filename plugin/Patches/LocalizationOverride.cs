using I2.Loc;
using System.Collections.Generic;
using UnityEngine;

namespace Alhxe.ChristianityExpanded.Patches
{
    /// <summary>
    /// Directly inserts our overrides into the I2 Localization dictionary at
    /// runtime. Bypasses Harmony patching, which we couldn't get to fire on
    /// LocalizationController.GetText for unclear reasons (BepInEx 5 + this
    /// game's Mono runtime).
    /// </summary>
    internal static class LocalizationOverride
    {
        // term -> { language -> translation }
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

        public static int Apply()
        {
            int applied = 0;
            if (LocalizationManager.Sources == null) return 0;

            foreach (var src in LocalizationManager.Sources)
            {
                if (src == null) continue;

                foreach (var pair in Overrides)
                {
                    string term = pair.Key;
                    var byLang = pair.Value;

                    TermData td = src.GetTermData(term);
                    if (td == null) continue;  // Term not in this source

                    foreach (var langPair in byLang)
                    {
                        int idx = src.GetLanguageIndex(langPair.Key);
                        if (idx < 0) continue;
                        if (idx < td.Languages.Length)
                        {
                            td.Languages[idx] = langPair.Value;
                            applied++;
                        }
                    }
                }

            }
            return applied;
        }
    }
}
