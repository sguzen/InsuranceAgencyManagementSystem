using System.ComponentModel.DataAnnotations;

namespace IAMS.Application.DTOs.Invoice
{
    public class CreateOrUpdateInvoiceDto
    {
        [Required(ErrorMessage = "Fatura numarası gereklidir")]
        public string InvoiceNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Poliçe seçimi gereklidir")]
        public int PolicyId { get; set; }

        [Required(ErrorMessage = "Müşteri seçimi gereklidir")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Fatura tarihi gereklidir")]
        public DateTime InvoiceDate { get; set; }

        [Required(ErrorMessage = "Vade tarihi gereklidir")]
        public DateTime DueDate { get; set; }

        [Range(0, 100, ErrorMessage = "Vergi oranı 0 ile 100 arasında olmalıdır")]
        public decimal TaxRate { get; set; } = 20;

        [Required(ErrorMessage = "Para birimi gereklidir")]
        public int CurrencyId { get; set; }

        public string? Notes { get; set; }

        public List<CreateOrUpdateInvoiceItemDto> Items { get; set; } = new();
    }
}
