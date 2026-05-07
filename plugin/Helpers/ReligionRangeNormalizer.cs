using HarmonyLib;
using NSEipix.Repository;
using NSMedieval.Model;
using NSMedieval.Repository;
using System.Linq;
using System.Reflection;

namespace Alhxe.ReligionsExpanded.Helpers
{
    /// <summary>
    /// Splits the 0..100 alignment scale into equal segments at runtime, one
    /// per religion currently registered in <see cref="ReligionRepository"/>.
    /// This means a mod author can add a religion with any (or no) `from`/`to`
    /// in JSON and the ranges sort themselves out — vanilla 2 religions stay
    /// at 0..49 and 50..100, three religions become 0..32 / 33..65 / 66..100,
    /// four become 0..24 / 25..49 / 50..74 / 75..100, etc.
    ///
    /// Order is the registration order returned by ReligionRepository, which
    /// keeps vanilla pagan first and vanilla christian second; modded
    /// religions land in whatever slot follows.
    /// </summary>
    internal static class ReligionRangeNormalizer
    {
        private static readonly FieldInfo FromField = AccessTools.Field(typeof(ReligionConfig), "from");
        private static readonly FieldInfo ToField   = AccessTools.Field(typeof(ReligionConfig), "to");

        private static bool _applied;

        public static void EnsureApplied()
        {
            if (_applied) return;

            var repo = Repository<ReligionRepository, ReligionConfig>.Instance;
            if (repo == null) return;

            var configs = repo.GetAllItems().Where(c => c != null).ToList();
            if (configs.Count == 0) return;
            if (FromField == null || ToField == null) { _applied = true; return; }

            int n = configs.Count;
            float step = 100f / n;
            for (int i = 0; i < n; i++)
            {
                float from = i == 0 ? 0f : i * step;
                float to   = i == n - 1 ? 100f : (i + 1) * step - 1f;
                FromField.SetValue(configs[i], from);
                ToField  .SetValue(configs[i], to);
            }

            _applied = true;
            Plugin.Log?.LogInfo($"[Ranges] normalized {n} religion(s) into equal segments.");
        }
    }
}
