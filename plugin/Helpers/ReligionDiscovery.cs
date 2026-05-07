using NSEipix.Base;
using NSEipix.Repository;
using NSMedieval.Model;
using NSMedieval.Repository;
using System.Collections.Generic;

namespace Alhxe.ReligionsExpanded.Helpers
{
    /// <summary>
    /// Reads the live ReligionRepository so the rest of the plugin can iterate
    /// every religion currently registered (vanilla + JSON content from any
    /// mod). Returned in ascending `From` order.
    /// </summary>
    internal static class ReligionDiscovery
    {
        public struct Religion
        {
            public string Id;
            public float From;
            public float To;
            public string DisplayName;       // primary (`religion_<id>_name` -> Cristianismo)
            public string AdjectiveName;     // adjective form (`general_<id>` -> Cristiano)
        }

        public static List<Religion> GetAll()
        {
            var result = new List<Religion>();

            var repo = Repository<ReligionRepository, ReligionConfig>.Instance;
            if (repo == null) return result;

            foreach (var cfg in repo.GetAllItems())
            {
                if (cfg == null) continue;
                result.Add(new Religion
                {
                    Id            = cfg.GetID(),
                    From          = cfg.From,
                    To            = cfg.To,
                    DisplayName   = Resolve($"religion_{cfg.GetID()}_name", cfg.GetID()),
                    AdjectiveName = Resolve($"general_{cfg.GetID()}", cfg.GetID()),
                });
            }

            result.Sort((a, b) => a.From.CompareTo(b.From));
            return result;
        }

        private static string Resolve(string key, string fallback)
        {
            string txt = I2.Loc.LocalizationManager.GetTranslation(key);
            return (string.IsNullOrEmpty(txt) || txt == key) ? fallback : txt;
        }
    }
}
