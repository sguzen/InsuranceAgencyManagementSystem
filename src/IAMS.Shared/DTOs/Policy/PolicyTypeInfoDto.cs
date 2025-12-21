namespace IAMS.Shared.DTOs.Policy
{
    public class PolicyTypeInfoDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal DefaultCommissionRate { get; set; }
        public bool IsActive { get; set; }
    }
}