namespace IAMS.Domain.Entities
{
    public class InvoiceItem : BaseEntity
    {
        public int InvoiceId { get; set; }
        public string Description { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Notes { get; set; }

        // Navigation properties
        public virtual Invoice Invoice { get; set; } = null!;

        // Business methods
        public void CalculateTotal()
        {
            TotalAmount = Quantity * UnitPrice;
        }

        public void UpdateQuantity(int newQuantity, string updatedBy)
        {
            if (newQuantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero");

            Quantity = newQuantity;
            CalculateTotal();
            UpdateAuditInfo(updatedBy);
        }

        public void UpdateUnitPrice(decimal newPrice, string updatedBy)
        {
            if (newPrice < 0)
                throw new ArgumentException("Unit price cannot be negative");

            UnitPrice = newPrice;
            CalculateTotal();
            UpdateAuditInfo(updatedBy);
        }
    }
}
