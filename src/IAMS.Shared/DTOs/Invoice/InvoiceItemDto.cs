namespace IAMS.Shared.DTOs.Invoice
{
    public class InvoiceItemDto
    {
        public int Id { get; set; }
        public int InvoiceId { get; set; }
        public string Description { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Notes { get; set; }
    }
}
