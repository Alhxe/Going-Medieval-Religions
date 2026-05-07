using Alhxe.ReligionsExpanded.Helpers;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace Alhxe.ReligionsExpanded
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid    = "alhxe.religions_expanded";
        public const string PluginName    = "Religions Expanded";
        public const string PluginVersion = "0.2.0";

        internal static ManualLogSource Log;
        private Harmony _harmony;

        private void Awake()
        {
            Log = Logger;
            Log.LogInfo($"{PluginName} {PluginVersion} loading...");

            // Load translations from the JSON mod's Localization folder so the
            // plugin and JSON content share a single source of truth.
            LocaleLoader.LoadFromMod();

            _harmony = new Harmony(PluginGuid);
            _harmony.PatchAll();

            // Defer SelectionStore.Load() until ReligionRepository is built
            // (it needs religion ids); see WorkerSpawnPatch for the trigger.

            int patched = 0;
            foreach (var m in _harmony.GetPatchedMethods())
            {
                Log.LogDebug($"  patched: {m.DeclaringType?.FullName}.{m.Name}");
                patched++;
            }

            Log.LogInfo($"{PluginName} loaded ({patched} method(s) patched).");
        }
    }
}
