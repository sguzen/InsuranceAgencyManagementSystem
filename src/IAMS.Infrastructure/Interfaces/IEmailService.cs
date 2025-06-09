using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Infrastructure.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendAsync(EmailMessage message);
        Task<bool> SendTemplateAsync(string templateName, object model, string toEmail, string? toName = null);
        Task<bool> SendBulkAsync(List<EmailMessage> messages);
        Task<bool> SendPolicyReminderAsync(string toEmail, string customerName, string policyNumber, DateTime expiryDate);
        Task<bool> SendClaimNotificationAsync(string toEmail, string customerName, string claimNumber, string status);
    }

    public class EmailMessage
    {
        public string ToEmail { get; set; } = string.Empty;
        public string? ToName { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsHtml { get; set; } = true;
        public List<EmailAttachment> Attachments { get; set; } = new();
        public Dictionary<string, string> Headers { get; set; } = new();
    }

    public class EmailAttachment
    {
        public string FileName { get; set; } = string.Empty;
        public byte[] Content { get; set; } = Array.Empty<byte>();
        public string ContentType { get; set; } = "application/octet-stream";
    }
}
