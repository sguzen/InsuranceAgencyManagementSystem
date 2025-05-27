namespace IAMS.Domain.Entities
{
    public class Customer : BaseEntity
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string TcNo { get; set; } = string.Empty; // Turkish Cypriot ID Number
        public DateTime DateOfBirth { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<Policy> Policies { get; set; } = new List<Policy>();
        public virtual ICollection<CustomerInsuranceCompany> CustomerInsuranceCompanies { get; set; } = new List<CustomerInsuranceCompany>();
    }
}