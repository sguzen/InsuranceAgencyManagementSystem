namespace IAMS.Application.DTOs.Policy
{
    /// <summary>
    /// Result of importing policies from Excel
    /// </summary>
    public class PolicyImportResultDto
    {
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<PolicyImportError> Errors { get; set; } = new();
        public List<int> ImportedPolicyIds { get; set; } = new();

        public bool HasErrors => Errors.Any();
    }

    public class PolicyImportError
    {
        public int RowNumber { get; set; }
        public string PolicyNumber { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string? FieldName { get; set; }
    }
}
