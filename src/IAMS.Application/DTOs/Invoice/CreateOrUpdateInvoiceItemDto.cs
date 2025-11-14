using System.ComponentModel.DataAnnotations;

namespace IAMS.Application.DTOs.Invoice
{
    public class CreateOrUpdateInvoiceItemDto
    {
        [Required(ErrorMessage = "Açıklama gereklidir")]
        public string Description { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Miktar en az 1 olmalıdır")]
        public int Quantity { get; set; } = 1;

        [Range(0.01, double.MaxValue, ErrorMessage = "Birim fiyat 0'dan büyük olmalıdır")]
        public decimal UnitPrice { get; set; }

        public string? Notes { get; set; }
    }
}
