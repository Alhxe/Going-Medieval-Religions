using BepInEx;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Alhxe.ReligionsExpanded.Helpers
{
    /// <summary>
    /// Persists which religions the player has marked as available for new
    /// scenarios, plus the probability a colonist starts unaligned. Saved as a
    /// tiny JSON file in BepInEx/config so it survives game restarts without
    /// touching the vanilla scenario data model. We can promote this to
    /// per-save state later.
    /// </summary>
    internal static class SelectionStore
    {
        // Default: roughly 6 of every 10 colonists start with no religion.
        // Tunable per-install via `unalignedChance` in selection.json.
        private const float DefaultUnalignedChance = 0.6f;

        private static readonly string ConfigDir =
            Path.Combine(Paths.ConfigPath, "ReligionsExpanded");
        private static readonly string ConfigFile =
            Path.Combine(ConfigDir, "selection.json");

        public static HashSet<string> Selected { get; private set; } = new HashSet<string>();
        public static float UnalignedChance { get; private set; } = DefaultUnalignedChance;
        public static bool Loaded { get; private set; }

        public static void Load()
        {
            try
            {
                if (!File.Exists(ConfigFile))
                {
                    // Default: enable every discovered religion.
                    Selected = new HashSet<string>(ReligionDiscovery.GetAll().Select(r => r.Id));
                    UnalignedChance = DefaultUnalignedChance;
                    Save();  // persist defaults so the user can edit them
                    Loaded = true;
                    Plugin.Log?.LogInfo($"[Selection] no saved file, defaulting to {Selected.Count} religions, unalignedChance={UnalignedChance:0.##}.");
                    return;
                }

                var data = JObject.Parse(File.ReadAllText(ConfigFile));
                Selected = new HashSet<string>(
                    data["selectedReligions"]?.Select(t => (string)t) ?? new string[0]);
                UnalignedChance = data["unalignedChance"] != null
                    ? UnityEngine.Mathf.Clamp01((float)data["unalignedChance"])
                    : DefaultUnalignedChance;
                Loaded = true;
                Plugin.Log?.LogInfo($"[Selection] loaded {Selected.Count} religions, unalignedChance={UnalignedChance:0.##}.");
            }
            catch (System.Exception ex)
            {
                Plugin.Log?.LogError($"[Selection] load failed: {ex.Message}");
                Selected = new HashSet<string>();
                UnalignedChance = DefaultUnalignedChance;
                Loaded = true;
            }
        }

        public static void Save()
        {
            try
            {
                Directory.CreateDirectory(ConfigDir);
                var obj = new JObject {
                    ["selectedReligions"] = new JArray(Selected.Select(s => (object)s).ToArray()),
                    ["unalignedChance"]   = UnalignedChance,
                };
                File.WriteAllText(ConfigFile, obj.ToString(Newtonsoft.Json.Formatting.Indented));
                Plugin.Log?.LogDebug($"[Selection] saved {Selected.Count} religions, unalignedChance={UnalignedChance:0.##}.");
            }
            catch (System.Exception ex)
            {
                Plugin.Log?.LogError($"[Selection] save failed: {ex.Message}");
            }
        }

        public static void Set(string id, bool enabled)
        {
            if (enabled) Selected.Add(id);
            else         Selected.Remove(id);
            Save();
        }

        public static void SetUnalignedChance(float v)
        {
            UnalignedChance = UnityEngine.Mathf.Clamp01(v);
            Save();
        }

        /// <summary>
        /// Pick a random religious alignment value compatible with the selected
        /// religions. Returns null when the colonist should be unaligned
        /// (selection empty, or random unaligned roll). Value is on the 0..100
        /// raw scale used by ReligionRepository.GetConfigForFaith.
        /// </summary>
        public static int? RollAlignment()
        {
            if (Selected.Count == 0) return null;
            if (UnityEngine.Random.value < UnalignedChance) return null;

            var religions = ReligionDiscovery.GetAll().Where(r => Selected.Contains(r.Id)).ToList();
            if (religions.Count == 0) return null;

            var pick = religions[UnityEngine.Random.Range(0, religions.Count)];
            // Random spot inside the religion's range so devotion varies per
            // colonist instead of every spawn landing on the boundary.
            float v = UnityEngine.Random.Range(pick.From, pick.To);
            return UnityEngine.Mathf.Clamp(UnityEngine.Mathf.RoundToInt(v), 0, 100);
        }

        /// <summary>Same as <see cref="RollAlignment"/> but returns 0..1 scale (HumanoidInfo).</summary>
        public static float? RollAlignmentNormalized()
        {
            int? raw = RollAlignment();
            return raw.HasValue ? (float?)(raw.Value / 100f) : null;
        }
    }
}
