namespace IAMS.Application.DTOs.Settings
{
    public class GeneralSettingsDto
    {
        public string CompanyName { get; set; } = string.Empty;
        public string CompanyAddress { get; set; } = string.Empty;
        public string CompanyPhone { get; set; } = string.Empty;
        public string CompanyEmail { get; set; } = string.Empty;
        public string TaxNumber { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;

        // Email Settings
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public string SmtpUsername { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public bool SmtpEnableSsl { get; set; } = true;
        public string EmailFromAddress { get; set; } = string.Empty;
        public string EmailFromName { get; set; } = string.Empty;

        // Regional Settings
        public string DefaultCurrency { get; set; } = "TRY";
        public string DateFormat { get; set; } = "dd/MM/yyyy";
        public string TimeFormat { get; set; } = "HH:mm";
        public string TimeZone { get; set; } = "Europe/Istanbul";
        public string Language { get; set; } = "tr-TR";
    }
}
