namespace IAMS.Domain.Interfaces
{
    public interface IAuditable
    {
        DateTime CreatedOn { get; set; }
        DateTime? ModifiedOn { get; set; }
        string CreatedBy { get; set; }
        string? ModifiedBy { get; set; }
    }
}