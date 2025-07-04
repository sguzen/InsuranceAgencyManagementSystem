namespace IAMS.Application.Models.Configuration
{
    public class CustomerCodeConfiguration
    {
        public string DefaultPrefix { get; set; } = "MUS";
        public int SequenceLength { get; set; } = 6;
        public int MaxRetryAttempts { get; set; } = 5;
        public bool UseYearInCode { get; set; } = true;
        public bool AllowTenantCustomPrefix { get; set; } = true;
    }
}