using Alhxe.ReligionsExpanded.Helpers;
using HarmonyLib;
using NSMedieval.UI;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Alhxe.ReligionsExpanded.Patches
{
    /// <summary>
    /// Rewrites the scenario summary text. Vanilla bakes the religion line as
    /// "Christian/Pagan: (X% / Y%)" using ForceReligion. With our selector the
    /// percentage is meaningless — we want a list of allowed religions instead.
    /// </summary>
    [HarmonyPatch(typeof(ScenarioView), "InitializeScenarioDetails")]
    internal static class ScenarioDetailsPatch
    {
        private static readonly FieldInfo DetailsField =
            AccessTools.Field(typeof(ScenarioView), "detailsSB");

        // Match `<token>/<token>: (X% / Y%)`. Same shape applies to gender too,
        // so we keep this loose and discriminate by content in the evaluator.
        private static readonly Regex BipolarLine =
            new Regex(@"^([^\s/]+)/([^\s/]+): \(\d+% / \d+%\)\s*$", RegexOptions.Multiline);

        [HarmonyPostfix]
        private static void Postfix(ScenarioView __instance)
        {
            if (DetailsField == null) return;
            var sb = DetailsField.GetValue(__instance) as StringBuilder;
            if (sb == null) return;

            var religions = ReligionDiscovery.GetAll();
            if (religions.Count == 0) return;

            // Build the set of adjective forms we'd recognize as a religion.
            var adjectives = new System.Collections.Generic.HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            foreach (var r in religions)
            {
                if (!string.IsNullOrEmpty(r.AdjectiveName)) adjectives.Add(r.AdjectiveName);
                if (!string.IsNullOrEmpty(r.DisplayName))   adjectives.Add(r.DisplayName);
            }

            string current = sb.ToString();
            string replacement = BuildReligionsLine(religions);

            string updated = BipolarLine.Replace(current, m =>
            {
                string a = m.Groups[1].Value;
                string b = m.Groups[2].Value;
                bool isReligion = adjectives.Contains(a) || adjectives.Contains(b);
                return isReligion ? replacement : m.Value;
            });

            if (updated != current)
            {
                sb.Clear();
                sb.Append(updated);
            }
        }

        private static string BuildReligionsLine(System.Collections.Generic.List<ReligionDiscovery.Religion> religions)
        {
            string label = LocText("scenario_edit_religion_title", "Religiones");
            if (religions.Count == 0) return $"{label}: -";

            // Mark which are enabled via SelectionStore (default: all).
            var selected = SelectionStore.Loaded ? SelectionStore.Selected : null;
            var bullets  = religions.Select(r =>
            {
                bool on = selected == null || selected.Contains(r.Id);
                return on ? r.DisplayName : $"<s>{r.DisplayName}</s>";
            });

            return $"{label}: ({string.Join(", ", bullets)})";
        }

        private static string LocText(string key, string fallback)
        {
            string txt = I2.Loc.LocalizationManager.GetTranslation(key);
            return string.IsNullOrEmpty(txt) || txt == key ? fallback : txt;
        }
    }
}
