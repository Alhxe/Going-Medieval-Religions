using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.IO;
using UnityEngine;

namespace Alhxe.ChristianityExpanded
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid    = "alhxe.christianity_expanded";
        public const string PluginName    = "Christianity Expanded";
        public const string PluginVersion = "0.1.0";

        internal static ManualLogSource Log;
        private static readonly string DebugFile =
            Path.Combine(Path.GetTempPath(), "ce-loc-debug.txt");

        private Harmony _harmony;

        private void Awake()
        {
            Log = Logger;
            Log.LogInfo($"{PluginName} {PluginVersion} loading...");
            Append("Plugin.Awake");

            _harmony = new Harmony(PluginGuid);
            _harmony.PatchAll();
            int patched = 0;
            foreach (var _ in _harmony.GetPatchedMethods()) patched++;
            Log.LogInfo($"Patched {patched} methods.");

            // Going Medieval disables the BepInEx_Manager component a second
            // after Awake (we discovered this empirically). Spawn our own
            // GameObject and attach a worker so we keep getting Update/etc.
            var go = new GameObject("ChristianityExpanded_Worker");
            DontDestroyOnLoad(go);
            go.AddComponent<Worker>();
            Append("Worker GameObject created");
        }

        internal static void Append(string msg)
        {
            try { File.AppendAllText(DebugFile, $"[{System.DateTime.Now:HH:mm:ss}] {msg}\n"); }
            catch { }
        }
    }

    /// <summary>Lives on a standalone DontDestroyOnLoad GameObject so the game can't disable it.</summary>
    internal class Worker : MonoBehaviour
    {
        private int _frame;
        private bool _localeApplied;

        private void Awake()    { Plugin.Append("Worker.Awake"); }
        private void OnEnable() { Plugin.Append("Worker.OnEnable"); }
        private void Start()    { Plugin.Append("Worker.Start"); }
        private void OnDisable(){ Plugin.Append("Worker.OnDisable"); }

        private void Update()
        {
            _frame++;
            if (_frame <= 3 || _frame == 60 || _frame == 600 || _frame == 6000)
            {
                int srcCount = I2.Loc.LocalizationManager.Sources?.Count ?? -1;
                Plugin.Append($"Worker.Update frame={_frame} sources={srcCount} applied={_localeApplied}");
            }

            if (_localeApplied) return;
            var sources = I2.Loc.LocalizationManager.Sources;
            if (sources == null || sources.Count == 0) return;

            try
            {
                int applied = Patches.LocalizationOverride.Apply();
                _localeApplied = true;
                Plugin.Append($"LocalizationOverride.Apply: {applied} terms (frame {_frame})");
                Plugin.Log?.LogInfo($"Localization overrides applied (frame {_frame}): {applied}");
            }
            catch (System.Exception ex)
            {
                _localeApplied = true;
                Plugin.Append($"LocalizationOverride.Apply failed: {ex.Message}");
            }
        }
    }
}
