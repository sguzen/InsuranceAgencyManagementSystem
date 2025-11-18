using IAMS.Application.Constants;

namespace IAMS.Application.Helpers
{
    public static class RoleTranslations
    {
        private static readonly Dictionary<string, string> _translations = new()
        {
            { RoleNames.SuperAdmin, "Süper Yönetici" },
            { RoleNames.TenantAdmin, "Kiracı Yöneticisi" },
            { RoleNames.Manager, "Yönetici" },
            { RoleNames.Agent, "Acente" },
            { RoleNames.Viewer, "Görüntüleyici" },
            { RoleNames.AccountingClerk, "Muhasebe Elemanı" },
            { RoleNames.ReportsViewer, "Rapor Görüntüleyici" }
        };

        public static string ToTurkish(string roleName)
        {
            return _translations.TryGetValue(roleName, out var translation)
                ? translation
                : roleName;
        }

        public static string ToEnglish(string turkishName)
        {
            var entry = _translations.FirstOrDefault(x => x.Value == turkishName);
            return entry.Key ?? turkishName;
        }

        public static Dictionary<string, string> GetAllTranslations()
        {
            return new Dictionary<string, string>(_translations);
        }

        public static List<string> GetTurkishRoleNames()
        {
            return _translations.Values.ToList();
        }

        public static List<string> GetEnglishRoleNames()
        {
            return _translations.Keys.ToList();
        }
    }
}
