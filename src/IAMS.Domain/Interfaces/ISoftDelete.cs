namespace IAMS.Domain.Interfaces
{
    public interface ISoftDeletable
    {
        bool IsDeleted { get; set; }
        DateTime? DeletedOn { get; set; }
        string? DeletedBy { get; set; }
    }
}