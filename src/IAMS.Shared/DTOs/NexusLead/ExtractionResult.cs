using System.ComponentModel.DataAnnotations;

namespace IAMS.Shared.DTOs.NexusLead
{
    /// <summary>
    /// Represents the incoming JSON schema from the Nexus server
    /// after image/document extraction.
    /// </summary>
    public class ExtractionResult
    {
        /// <summary>
        /// Indicates if the extraction was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// The extracted data from the document/image.
        /// </summary>
        public ExtractedData ExtractedData { get; set; } = new ExtractedData();

        /// <summary>
        /// Any error message if extraction failed.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// The source of the lead (e.g., "Telegram Bot").
        /// </summary>
        public string Source { get; set; } = "Telegram Bot";
    }

    /// <summary>
    /// Contains the individual fields extracted from the document.
    /// </summary>
    public class ExtractedData
    {
        /// <summary>
        /// Full name of the person (may be split into FirstName/LastName later).
        /// </summary>
        public string? FullName { get; set; }

        /// <summary>
        /// First name of the person.
        /// </summary>
        public string? FirstName { get; set; }

        /// <summary>
        /// Last name (surname) of the person.
        /// </summary>
        public string? LastName { get; set; }

        /// <summary>
        /// TRNC / KKTC Identity Number (Kimlik No).
        /// </summary>
        public string? TrncIdNumber { get; set; }

        /// <summary>
        /// Date of birth as an ISO string or formatted date.
        /// </summary>
        public string? DateOfBirth { get; set; }

        /// <summary>
        /// Place of birth or nationality.
        /// </summary>
        public string? PlaceOfBirth { get; set; }

        /// <summary>
        /// Email address.
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Primary phone number.
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// Mobile phone number with optional country code.
        /// </summary>
        public string? MobilePhoneNumber { get; set; }

        /// <summary>
        /// Address line 1.
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// Address line 1 (explicit).
        /// </summary>
        public string? Address1 { get; set; }

        /// <summary>
        /// Gender: Male or Female.
        /// </summary>
        public string? Gender { get; set; }

        /// <summary>
        /// Document type that was processed (e.g., ID card, passport).
        /// </summary>
        public string? DocumentType { get; set; }

        /// <summary>
        /// Raw confidence score from the extraction engine (0.0 - 1.0).
        /// </summary>
        public double? ConfidenceScore { get; set; }
    }
}
