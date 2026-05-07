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

            label = HeaderLabel();
        }

        private static string HeaderLabel()
        {
            string lang = I2.Loc.LocalizationManager.CurrentLanguage ?? "English";
            return lang == "Spanish" ? "Religiones" : "Religions";
        }

        [HarmonyPostfix]
        private static void Postfix(ScenarioEditIntView __instance, string label, IntRange minMaxRange, int currentValue, string suffix)
        {
            if (__instance == null) return;
            // Only act on rows whose label we just rewrote.
            var religions = ReligionDiscovery.GetAll();
            if (religions.Count == 0) return;
            if (!string.Equals(label, HeaderLabel())) return;

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
            // Make sure SelectionStore reflects whatever's saved on disk before
            // we render the row.
            if (!SelectionStore.Loaded) SelectionStore.Load();

            // Hide the vanilla numeric input + percent suffix.
            var intInput    = IntInputField?.GetValue(view) as TMP_InputField;
            var suffixLabel = SuffixLabelField?.GetValue(view) as TMP_Text;
            if (intInput?.gameObject != null)    intInput.gameObject.SetActive(false);
            if (suffixLabel?.gameObject != null) suffixLabel.gameObject.SetActive(false);

            // Anchor in the input's parent and TAKE the input's sibling slot
            // so the row's right-side info-book / delete-button stay where
            // they were. Keeps the layout consistent with other rows.
            var parent = (intInput != null ? intInput.transform.parent : view.transform);

            var btnGo = new GameObject("ReligionsExpanded_Selector");
            btnGo.transform.SetParent(parent, worldPositionStays: false);
            if (intInput != null)
            {
                btnGo.transform.SetSiblingIndex(intInput.transform.GetSiblingIndex());
            }

            var btnRT = btnGo.AddComponent<RectTransform>();
            btnRT.sizeDelta = new Vector2(220, 30);

            // Styled like the other inputs: dark fill + thin border.
            var btnBg   = btnGo.AddComponent<Image>();
            btnBg.color = new Color(0.13f, 0.13f, 0.17f, 1f);

            var border = btnGo.AddComponent<Outline>();
            border.effectColor    = new Color(0.42f, 0.55f, 0.65f, 0.85f);
            border.effectDistance = new Vector2(1, -1);

            var btn = btnGo.AddComponent<Button>();
            btn.targetGraphic = btnBg;

            // Label inside the button.
            var labelGo = new GameObject("label");
            labelGo.transform.SetParent(btnGo.transform, worldPositionStays: false);
            var labelRT = labelGo.AddComponent<RectTransform>();
            labelRT.anchorMin = Vector2.zero;
            labelRT.anchorMax = Vector2.one;
            labelRT.offsetMin = new Vector2(10, 0);
            labelRT.offsetMax = new Vector2(-10, 0);

            var labelTmp = labelGo.AddComponent<TextMeshProUGUI>();
            labelTmp.fontSize         = 18;
            labelTmp.color            = Color.white;
            labelTmp.alignment        = TextAlignmentOptions.MidlineLeft;
            labelTmp.enableAutoSizing = true;
            labelTmp.fontSizeMin      = 10;
            labelTmp.fontSizeMax      = 18;
            labelTmp.enableWordWrapping = false;
            labelTmp.overflowMode     = TextOverflowModes.Ellipsis;
            void RefreshLabel()
            {
                var sel = SelectionStore.Selected;
                labelTmp.text = sel.Count == 0
                    ? NoneLabel()
                    : string.Join(", ", religions.Where(r => sel.Contains(r.Id)).Select(r => r.DisplayName));
            }
            RefreshLabel();

            // Popup parented to the selector button — keeps positioning
            // straightforward (anchor below the button) and inherits the
            // existing canvas without any coordinate conversion. Outside-
            // click-close is handled by a small polling MonoBehaviour.
            var popupGo = BuildPopupSimple(btnGo.GetComponent<RectTransform>(), religions, RefreshLabel);
            popupGo.SetActive(false);

            var dismisser = popupGo.AddComponent<OutsideClickDismisser>();
            dismisser.Init(popupGo, btnGo.GetComponent<RectTransform>());

            btn.onClick.AddListener(() =>
            {
                bool willOpen = !popupGo.activeSelf;
                Plugin.Log?.LogInfo($"[MultiReligion] selector clicked, willOpen={willOpen}");
                popupGo.SetActive(willOpen);
            });
        }

        private static string NoneLabel()
        {
            string lang = I2.Loc.LocalizationManager.CurrentLanguage ?? "English";
            return lang == "Spanish" ? "(ninguna)" : "(none)";
        }

        private static GameObject BuildPopupSimple(RectTransform anchorButton, List<ReligionDiscovery.Religion> religions, System.Action onChange)
        {
            var popup = new GameObject("ReligionsExpanded_Popup");
            popup.transform.SetParent(anchorButton, worldPositionStays: false);
            popup.transform.SetAsLastSibling();

            var rt = popup.AddComponent<RectTransform>();
            rt.pivot     = new Vector2(0, 1);
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(0, 0);
            rt.anchoredPosition = new Vector2(0, -2);
            rt.sizeDelta = new Vector2(220, 30 * religions.Count + 12);

            // Render above sibling sub-canvases.
            var canvas = popup.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder    = 32000;
            popup.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            var bg = popup.AddComponent<Image>();
            bg.color = new Color(0.12f, 0.12f, 0.16f, 0.97f);

            var border = popup.AddComponent<Outline>();
            border.effectColor    = new Color(0.42f, 0.55f, 0.65f, 0.85f);
            border.effectDistance = new Vector2(1, -1);

            var layout = popup.AddComponent<VerticalLayoutGroup>();
            layout.padding              = new RectOffset(8, 8, 6, 6);
            layout.spacing              = 4f;
            layout.childForceExpandWidth = true;
            layout.childAlignment       = TextAnchor.UpperLeft;

            foreach (var r in religions)
            {
                BuildPopupRow(popup.transform, r, onChange);
            }
            return popup;
        }

        private static void BuildPopupRow(Transform parent, ReligionDiscovery.Religion religion, System.Action onChange)
        {
            var rowGo = new GameObject($"row_{religion.Id}");
            rowGo.transform.SetParent(parent, worldPositionStays: false);

            var row = rowGo.AddComponent<RectTransform>();
            row.sizeDelta = new Vector2(0, 24);

            var hl = rowGo.AddComponent<HorizontalLayoutGroup>();
            hl.spacing               = 8f;
            hl.childForceExpandWidth = false;
            hl.childForceExpandHeight = true;
            hl.childAlignment        = TextAnchor.MiddleLeft;

            var box = new GameObject("box");
            box.transform.SetParent(rowGo.transform, worldPositionStays: false);
            var boxRT  = box.AddComponent<RectTransform>();
            boxRT.sizeDelta = new Vector2(20, 20);
            var boxImg = box.AddComponent<Image>();

            // Checkmark child — separate TMP so we can show / hide it.
            var checkGo = new GameObject("check");
            checkGo.transform.SetParent(box.transform, worldPositionStays: false);
            var checkRT = checkGo.AddComponent<RectTransform>();
            checkRT.anchorMin = Vector2.zero;
            checkRT.anchorMax = Vector2.one;
            checkRT.offsetMin = Vector2.zero;
            checkRT.offsetMax = Vector2.zero;
            var checkTmp = checkGo.AddComponent<TextMeshProUGUI>();
            checkTmp.text      = "✓";
            checkTmp.fontSize  = 18;
            checkTmp.color     = Color.white;
            checkTmp.alignment = TextAlignmentOptions.Center;
            checkTmp.fontStyle = FontStyles.Bold;

            var toggle = rowGo.AddComponent<Toggle>();
            toggle.targetGraphic = boxImg;
            toggle.isOn          = SelectionStore.Selected.Contains(religion.Id);

            void Repaint(bool on)
            {
                boxImg.color = on ? new Color(0.45f, 0.7f, 0.4f, 1f)
                                  : new Color(0.3f, 0.3f, 0.3f, 1f);
                checkGo.SetActive(on);
            }
            Repaint(toggle.isOn);

            toggle.onValueChanged.AddListener(on =>
            {
                Repaint(on);
                SelectionStore.Set(religion.Id, on);
                onChange?.Invoke();
            });

            var lblGo = new GameObject("label");
            lblGo.transform.SetParent(rowGo.transform, worldPositionStays: false);
            var lbl = lblGo.AddComponent<TextMeshProUGUI>();
            lbl.text      = religion.DisplayName;
            lbl.fontSize  = 16;
            lbl.color     = Color.white;
            lbl.alignment = TextAlignmentOptions.MidlineLeft;
        }
    }

    /// <summary>Closes the popup when the user clicks anywhere outside its
    /// rect (or outside the selector button that opens it).</summary>
    internal class OutsideClickDismisser : MonoBehaviour
    {
        private GameObject _popup;
        private RectTransform _popupRT;
        private RectTransform _anchorRT;
        private int _ignoreFirstFrame;

        public void Init(GameObject popup, RectTransform anchor)
        {
            _popup    = popup;
            _popupRT  = popup.GetComponent<RectTransform>();
            _anchorRT = anchor;
        }

        private void OnEnable() { _ignoreFirstFrame = 1; }

        private void Update()
        {
            if (_ignoreFirstFrame > 0) { _ignoreFirstFrame--; return; }
            if (!Input.GetMouseButtonDown(0)) return;

            Vector2 pos = Input.mousePosition;
            if (RectTransformUtility.RectangleContainsScreenPoint(_popupRT, pos)) return;
            if (RectTransformUtility.RectangleContainsScreenPoint(_anchorRT, pos)) return;
            _popup.SetActive(false);
        }
    }
}
