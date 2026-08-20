using System.Text.RegularExpressions;

namespace IAMS.Shared.Validation
{
    /// <summary>
    /// Rules for the code an insurance company assigns to an agency in its own system
    /// (the "ackod" column in the insurer's policy database).
    /// Each insurer assigns its own code, so it is stored per agency–insurance-company link
    /// (<c>AgencyInsuranceCompany.AgencyCode</c>) and used to filter policies on import.
    /// </summary>
    public static class AgencyCodeRules
    {
        public const int MaxLength = 10;

        /// <summary>Letters and digits only, 1–10 characters. Kept strict because the value is used in a SQL filter.</summary>
        public const string Pattern = "^[A-Za-z0-9]{1,10}$";

        private static readonly Regex Regex = new(Pattern, RegexOptions.Compiled);

        public static bool IsValid(string? code) =>
            !string.IsNullOrEmpty(code) && Regex.IsMatch(code);

        /// <summary>
        /// Trims the input. Empty or whitespace normalizes to <c>null</c> (meaning "not configured")
        /// and is accepted. A present but malformed value is rejected with an error message.
        /// </summary>
        public static bool TryNormalize(string? input, out string? normalized, out string? error)
        {
            normalized = string.IsNullOrWhiteSpace(input) ? null : input.Trim();
            error = null;

            if (normalized == null || Regex.IsMatch(normalized))
                return true;

            error = $"Agency code '{normalized}' is invalid. Use 1-{MaxLength} letters or digits only.";
            normalized = null;
            return false;
        }
    }
}
