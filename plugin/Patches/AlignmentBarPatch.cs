using HarmonyLib;
using NSEipix.Base;
using NSEipix.Repository;
using NSMedieval.Model;
using NSMedieval.Repository;
using NSMedieval.State;
using NSMedieval.StatsSystem;
using NSMedieval.UI;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace Alhxe.ReligionsExpanded.Patches
{
    /// <summary>
    /// Reinterprets the ReligiousAlignment bar in a colonist's bio panel.
    /// Vanilla draws it bipolar (pagan-left / christian-right). We want it to
    /// show *which* religion the colonist follows plus how devout they are
    /// inside that religion.
    ///
    ///   no religion           -> empty bar, label "Sin religión"
    ///   inside [from..to]     -> right-side bar fills 0..1 within the range,
    ///                            label is the religion adjective form
    ///                            (e.g. "Cristiano (75%)").
    /// </summary>
    [HarmonyPatch(typeof(AlignmentLayoutItemView), nameof(AlignmentLayoutItemView.SetAlignmentData))]
    internal static class AlignmentBarPatch
    {
        // GroupItems indices match the private constants in vanilla
        // AlignmentLayoutItemView. Mirrored here so we don't reflect.
        private const int SkillSliderLeft  = 1;
        private const int SkillSliderRight = 2;
        private const int TrendArrowLeft   = 3;
        private const int TrendArrowRight  = 4;

        private static readonly FieldInfo LeftBarField  = AccessTools.Field(typeof(AlignmentLayoutItemView), "leftBar");
        private static readonly FieldInfo RightBarField = AccessTools.Field(typeof(AlignmentLayoutItemView), "rightBar");

        [HarmonyPostfix]
        private static void Postfix(AlignmentLayoutItemView __instance, StatType alignmentType, float value, HumanoidInstance humanoid)
        {
            if (alignmentType != StatType.ReligiousAlignment) return;

            float raw = value * 100f;
            ReligionConfig cfg = Repository<ReligionRepository, ReligionConfig>.Instance?.GetConfigForFaith(raw);

            string label;
            float devotion;

            if (cfg == null)
            {
                label    = LocText("religion_unaligned_name", "Sin religión");
                devotion = 0f;
            }
            else
            {
                string id   = cfg.GetID();
                string name = LocText($"general_{id}", id);
                float span  = cfg.To - cfg.From;
                devotion = span > 0 ? Mathf.Clamp01((raw - cfg.From) / span) : 1f;
                label    = $"{name} ({Mathf.RoundToInt(devotion * 100f)}%)";
            }

            __instance.SetText(label);

            // Disable the underlying Slider components so they don't keep
            // overwriting the Image fillAmount we configure below.
            var items = __instance.GroupItems;
            if (items != null)
            {
                if (items.Count > TrendArrowLeft)  items[TrendArrowLeft] .SetActive(false);
                if (items.Count > TrendArrowRight) items[TrendArrowRight].SetActive(false);

                // Disable the Slider components only — keep their GameObjects
                // active so our overlay children remain visible.
                if (items.Count > SkillSliderLeft)
                {
                    var s = items[SkillSliderLeft].GetComponent<Slider>();
                    if (s != null) s.enabled = false;
                }
                if (items.Count > SkillSliderRight)
                {
                    var s = items[SkillSliderRight].GetComponent<Slider>();
                    if (s != null) s.enabled = false;
                }
            }

            ConfigureUnipolarBar(__instance, devotion);
        }

        /// <summary>
        /// Repurposes the vanilla bipolar widget into a single 0..100% bar
        /// by adding our own Image overlay that spans the full bar area and
        /// fills horizontally from the left edge.
        /// </summary>
        private static void ConfigureUnipolarBar(AlignmentLayoutItemView view, float devotion)
        {
            var leftBar  = LeftBarField ?.GetValue(view) as Image;
            var rightBar = RightBarField?.GetValue(view) as Image;

            float clamped = Mathf.Clamp01(devotion);
            if (TryMergeHalvesIntoWideBar(leftBar, rightBar))
            {
                // Merged: drive only the left bar across the full width.
                ApplyUnipolarFill(leftBar, clamped);
            }
            else
            {
                // Couldn't merge (halves live under different parents): fall
                // back to filling each half independently so the visible bar
                // still tracks devotion 0..1 left-to-right.
                ApplyUnipolarFill(leftBar,  Mathf.Clamp01(clamped * 2f));
                ApplyUnipolarFill(rightBar, Mathf.Clamp01((clamped - 0.5f) * 2f));
            }
        }

        private static bool TryMergeHalvesIntoWideBar(Image leftBar, Image rightBar)
        {
            if (leftBar == null || rightBar == null) return false;

            var leftHalf  = leftBar.transform.parent  as RectTransform;
            var rightHalf = rightBar.transform.parent as RectTransform;
            if (leftHalf == null || rightHalf == null) return false;
            if (leftHalf.parent != rightHalf.parent) return false;

            // Expand left half to cover from its current left edge to the
            // right half's right edge.
            leftHalf.anchorMax = new Vector2(rightHalf.anchorMax.x, leftHalf.anchorMax.y);
            leftHalf.offsetMax = new Vector2(rightHalf.offsetMax.x, leftHalf.offsetMax.y);

            // Hide the right half so its inner edge doesn't read as a divider.
            if (rightHalf.gameObject.activeSelf) rightHalf.gameObject.SetActive(false);
            return true;
        }

        /// <summary>
        /// Reuses the vanilla half-bar Image (so its position, sprite and
        /// size stay native) but flips it to a left-to-right horizontal
        /// fill driven by our own fillAmount. The vanilla Slider component
        /// is disabled so it doesn't keep overwriting fillAmount each tick.
        /// </summary>
        private static void ApplyUnipolarFill(Image bar, float fill)
        {
            if (bar == null) return;

            // Walk up to the slider that owns this bar (its fillRect) and
            // disable the script so it stops driving the bar's anchors.
            var slider = bar.GetComponentInParent<Slider>();
            if (slider != null && slider.enabled) slider.enabled = false;

            // Reset the bar's anchors to span its half-container fully so
            // the previous slider-driven anchors don't clip the fill.
            var rt = bar.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            bar.type        = Image.Type.Filled;
            bar.fillMethod  = Image.FillMethod.Horizontal;
            bar.fillOrigin  = (int)Image.OriginHorizontal.Left;
            bar.fillAmount  = Mathf.Clamp01(fill);
            if (!bar.enabled) bar.enabled = true;
        }

        /// <summary>
        private static string LocText(string key, string fallback)
        {
            string txt = I2.Loc.LocalizationManager.GetTranslation(key);
            return string.IsNullOrEmpty(txt) || txt == key ? fallback : txt;
        }
    }
}
