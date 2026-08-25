namespace IAMS.Shared.DTOs.Agency
{
    public class AgencyInsuranceCompanyDto
    {
        public int Id { get; set; }
        public int AgencyId { get; set; }
        public int InsuranceCompanyId { get; set; }
        public string InsuranceCompanyName { get; set; } = string.Empty;
        public string InsuranceCompanyCode { get; set; } = string.Empty;
        /// <summary>Agency code assigned by this insurer (insurer's "ackod" for this agency).</summary>
        public string? AgencyCode { get; set; }
        public string? DbServer { get; set; }
        public string? DbName { get; set; }
        public string? DbUsername { get; set; }
        public bool HasPassword { get; set; }
        public string? ConnectionString { get; set; }
        public bool IsActive { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }

    public class CreateAgencyInsuranceCompanyDto
    {
        public int InsuranceCompanyId { get; set; }
        public string InsuranceCompanyName { get; set; } = string.Empty;
        public string InsuranceCompanyCode { get; set; } = string.Empty;
        /// <summary>Agency code assigned by this insurer (insurer's "ackod" for this agency).</summary>
        public string? AgencyCode { get; set; }
        public string? DbServer { get; set; }
        public string? DbName { get; set; }
        public string? DbUsername { get; set; }
        public string? DbPassword { get; set; }
        public string? ConnectionString { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateAgencyInsuranceCompanyDto
    {
        /// <summary>Agency code assigned by this insurer (insurer's "ackod" for this agency).</summary>
        public string? AgencyCode { get; set; }
        public string? DbServer { get; set; }
        public string? DbName { get; set; }
        public string? DbUsername { get; set; }
        public string? DbPassword { get; set; }
        public string? ConnectionString { get; set; }
        public bool IsActive { get; set; }
        public string? Notes { get; set; }
    }

    public class InsuranceCompanySummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
