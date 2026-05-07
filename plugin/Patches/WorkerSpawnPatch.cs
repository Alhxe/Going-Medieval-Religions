using Alhxe.ReligionsExpanded.Helpers;
using HarmonyLib;
using NSMedieval;
using NSMedieval.State;
using System.Collections.Generic;
using NSEipix.Model;
using NSMedieval.Model;

namespace Alhxe.ReligionsExpanded.Patches
{
    /// <summary>
    /// Vanilla `GetRandomReligiousAlignment` returns a binary -1 (pagan) or 1
    /// (christian) bucket, used only to pick a Background/BackStory. We
    /// expand that to roll against the player's chosen religions, plus an
    /// "unaligned" outcome.
    /// </summary>
    [HarmonyPatch(typeof(WorkerGenerator), "GetRandomReligiousAlignment")]
    internal static class GetRandomReligiousAlignmentPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(ref int __result)
        {
            if (!SelectionStore.Loaded) SelectionStore.Load();

            int? rolled = SelectionStore.RollAlignment();
            // The downstream BackgroundRepository only understands -1 / 1, so
            // we collapse our roll back to that scale here. The actual
            // float-precision alignment lands in HumanoidInfo via the
            // generator postfix below.
            if (!rolled.HasValue)
            {
                __result = 0;  // BackgroundRepository default bucket
            }
            else
            {
                __result = rolled.Value >= 50 ? 1 : -1;
            }
            return false;  // skip original method
        }
    }

    /// <summary>
    /// Vanilla `GenerateWorker` constructs a HumanoidInfo with hardcoded
    /// religiousAlignment = 0.5f. Background/BackStory then nudge it slightly,
    /// but the result always lands near 0.5 — i.e. always Christianity at 0%
    /// devotion. We override the final value with our roll so colonists
    /// actually represent the player's selected religions.
    /// </summary>
    [HarmonyPatch(typeof(WorkerGenerator), nameof(WorkerGenerator.GenerateWorker),
                  new[] { typeof(string), typeof(string), typeof(List<SerializableIdValuePair>) })]
    internal static class GenerateWorkerPatch
    {
        [HarmonyPostfix]
        private static void Postfix(HumanoidInstance __result)
        {
            if (__result?.Info == null) return;
            if (!SelectionStore.Loaded) SelectionStore.Load();

            float? rolled = SelectionStore.RollAlignmentNormalized();
            __result.Info.ReligiousAlignment = rolled.HasValue
                ? rolled.Value
                : -1f;  // sentinel below 0..1 -> GetConfigForFaith returns null -> unaligned
        }
    }
}
