using Alhxe.ReligionsExpanded.Helpers;
using HarmonyLib;
using NSEipix.Model;
using NSMedieval.UI.ScenarioEditor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Alhxe.ReligionsExpanded.Patches
{
    /// <summary>
    /// Religion-row UI swap: when the scenario editor builds the religion
    /// constraint with the vanilla int slider, we rewrite the label to list
    /// every discovered religion AND replace the numeric input with one toggle
    /// per religion (selectable / not selectable). This is the visible
    /// foundation for the multi-religion system.
    /// </summary>
    [HarmonyPatch(typeof(ScenarioEditIntView), nameof(ScenarioEditIntView.SetDefaults),
        new[] { typeof(string), typeof(IntRange), typeof(int), typeof(string) })]
    internal static class MultiReligionScenario
    {
        // Track which view items we've already converted so reopens don't
        // duplicate the toggle row.
        private static readonly HashSet<int> _converted = new HashSet<int>();

        // Reflection helpers
        private static readonly FieldInfo IntInputField =
            AccessTools.Field(typeof(ScenarioEditIntView), "intInput");
        private static readonly FieldInfo SuffixLabelField =
            AccessTools.Field(typeof(ScenarioEditIntView), "suffixLabel");

        [HarmonyPrefix]
        private static void Prefix(ref string label, IntRange minMaxRange, int currentValue, string suffix)
        {
            if (string.IsNullOrEmpty(label) || !label.Contains("/")) return;

            var religions = ReligionDiscovery.GetAll();
            if (religions.Count == 0) return;

            var parts = label.Split('/');
            bool isReligionRow = parts.Length >= 2 && parts.All(p =>
            {
                string trimmed = p.Trim();
                return religions.Any(r =>
                    string.Equals(r.DisplayName,   trimmed, System.StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(r.AdjectiveName, trimmed, System.StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(r.Id,            trimmed, System.StringComparison.OrdinalIgnoreCase));
            });
            if (!isReligionRow) return;

            label = string.Join(" / ", religions.Select(r => r.DisplayName));
        }

        [HarmonyPostfix]
        private static void Postfix(ScenarioEditIntView __instance, string label, IntRange minMaxRange, int currentValue, string suffix)
        {
            if (__instance == null) return;
            // Only act on rows whose label we just rewrote — i.e. the religion row.
            var religions = ReligionDiscovery.GetAll();
            if (religions.Count == 0) return;
            string expected = string.Join(" / ", religions.Select(r => r.DisplayName));
            if (!string.Equals(label, expected)) return;

            int id = __instance.GetInstanceID();
            if (_converted.Contains(id)) return;
            _converted.Add(id);

            try
            {
                ConvertToToggles(__instance, religions);
                Plugin.Log?.LogInfo($"[MultiReligion] toggle row built ({religions.Count} religions).");
            }
            catch (System.Exception ex)
            {
                Plugin.Log?.LogError($"[MultiReligion] toggle build failed: {ex}");
            }
        }

        private static void ConvertToToggles(ScenarioEditIntView view, List<ReligionDiscovery.Religion> religions)
        {
            // Hide the vanilla numeric input + percent suffix.
            var intInput     = IntInputField?.GetValue(view) as TMP_InputField;
            var suffixLabel  = SuffixLabelField?.GetValue(view) as TMP_Text;
            if (intInput?.gameObject != null) intInput.gameObject.SetActive(false);
            if (suffixLabel?.gameObject != null) suffixLabel.gameObject.SetActive(false);

            // Build a row container next to the input.
            var parent = (intInput != null ? intInput.transform.parent : view.transform);
            var row    = new GameObject("ReligionsExpanded_TogglesRow");
            row.transform.SetParent(parent, worldPositionStays: false);

            var layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.childForceExpandWidth  = false;
            layout.childForceExpandHeight = false;
            layout.spacing                = 14f;
            layout.childAlignment         = TextAnchor.MiddleLeft;
            layout.padding                = new RectOffset(0, 0, 0, 0);

            row.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            foreach (var r in religions)
            {
                BuildToggle(row.transform, r);
            }
        }

        private static void BuildToggle(Transform parent, ReligionDiscovery.Religion religion)
        {
            var go = new GameObject($"toggle_{religion.Id}");
            go.transform.SetParent(parent, worldPositionStays: false);

            var horizontal = go.AddComponent<HorizontalLayoutGroup>();
            horizontal.spacing               = 4f;
            horizontal.childForceExpandWidth = false;

            // Checkbox (very simple visual — a square Image that toggles fill colour).
            var box = new GameObject("box");
            box.transform.SetParent(go.transform, worldPositionStays: false);
            var boxRT  = box.AddComponent<RectTransform>();
            boxRT.sizeDelta = new Vector2(20, 20);
            var boxImg = box.AddComponent<Image>();

            var toggle = go.AddComponent<Toggle>();
            toggle.targetGraphic = boxImg;
            toggle.isOn          = true;

            void Repaint(bool on)
            {
                boxImg.color = on ? new Color(0.45f, 0.7f, 0.4f, 1f)
                                  : new Color(0.25f, 0.25f, 0.25f, 1f);
            }
            Repaint(true);
            toggle.onValueChanged.AddListener(Repaint);

            // Label
            var labelGo = new GameObject("label");
            labelGo.transform.SetParent(go.transform, worldPositionStays: false);
            var tmp = labelGo.AddComponent<TextMeshProUGUI>();
            tmp.text     = religion.DisplayName;
            tmp.fontSize = 18;
            tmp.color    = Color.white;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            var fitter = labelGo.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit   = ContentSizeFitter.FitMode.PreferredSize;
        }
    }
}
