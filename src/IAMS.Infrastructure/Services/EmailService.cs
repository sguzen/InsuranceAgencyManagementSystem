// IAMS.Infrastructure/Services/EmailService.cs
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using IAMS.Infrastructure.Interfaces;

namespace IAMS.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly EmailSettings _emailSettings;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _emailSettings = _configuration.GetSection("EmailSettings").Get<EmailSettings>() ?? new EmailSettings();
        }

        public async Task<bool> SendAsync(EmailMessage message)
        {
            try
            {
                using var client = CreateSmtpClient();
                using var mailMessage = CreateMailMessage(message);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent successfully to {ToEmail}", message.ToEmail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {ToEmail}", message.ToEmail);
                return false;
            }
        }

        public async Task<bool> SendTemplateAsync(string templateName, object model, string toEmail, string? toName = null)
        {
            try
            {
                var template = await LoadTemplateAsync(templateName);
                var renderedTemplate = RenderTemplate(template, model);

                var message = new EmailMessage
                {
                    ToEmail = toEmail,
                    ToName = toName,
                    Subject = renderedTemplate.Subject,
                    Body = renderedTemplate.Body,
                    IsHtml = true
                };

                return await SendAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send template email {TemplateName} to {ToEmail}", templateName, toEmail);
                return false;
            }
        }

        public async Task<bool> SendBulkAsync(List<EmailMessage> messages)
        {
            var results = new List<bool>();

            foreach (var message in messages)
            {
                var result = await SendAsync(message);
                results.Add(result);

                // Add small delay to prevent overwhelming the SMTP server
                await Task.Delay(100);
            }

            var successCount = results.Count(r => r);
            _logger.LogInformation("Bulk email completed: {SuccessCount}/{TotalCount} sent successfully",
                successCount, messages.Count);

            return successCount == messages.Count;
        }

        public async Task<bool> SendPolicyReminderAsync(string toEmail, string customerName, string policyNumber, DateTime expiryDate)
        {
            var model = new
            {
                CustomerName = customerName,
                PolicyNumber = policyNumber,
                ExpiryDate = expiryDate.ToString("dd/MM/yyyy"),
                DaysToExpiry = (expiryDate - DateTime.Now).Days,
                AgencyName = _emailSettings.AgencyName,
                ContactPhone = _emailSettings.ContactPhone,
                ContactEmail = _emailSettings.ContactEmail
            };

            return await SendTemplateAsync("policy-reminder", model, toEmail, customerName);
        }

        public async Task<bool> SendClaimNotificationAsync(string toEmail, string customerName, string claimNumber, string status)
        {
            var model = new
            {
                CustomerName = customerName,
                ClaimNumber = claimNumber,
                Status = status,
                StatusDate = DateTime.Now.ToString("dd/MM/yyyy"),
                AgencyName = _emailSettings.AgencyName,
                ContactPhone = _emailSettings.ContactPhone,
                ContactEmail = _emailSettings.ContactEmail
            };

            return await SendTemplateAsync("claim-notification", model, toEmail, customerName);
        }

        private SmtpClient CreateSmtpClient()
        {
            var client = new SmtpClient(_emailSettings.SmtpHost, _emailSettings.SmtpPort)
            {
                EnableSsl = _emailSettings.EnableSsl,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password)
            };

            return client;
        }

        private MailMessage CreateMailMessage(EmailMessage message)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                Subject = message.Subject,
                Body = message.Body,
                IsBodyHtml = message.IsHtml
            };

            mailMessage.To.Add(new MailAddress(message.ToEmail, message.ToName ?? message.ToEmail));

            // Add attachments
            foreach (var attachment in message.Attachments)
            {
                var stream = new MemoryStream(attachment.Content);
                var mailAttachment = new Attachment(stream, attachment.FileName, attachment.ContentType);
                mailMessage.Attachments.Add(mailAttachment);
            }

            // Add custom headers
            foreach (var header in message.Headers)
            {
                mailMessage.Headers.Add(header.Key, header.Value);
            }

            return mailMessage;
        }

        private async Task<EmailTemplate> LoadTemplateAsync(string templateName)
        {
            // In a real implementation, you might load from database or file system
            // For now, return hardcoded templates for insurance agency
            return templateName.ToLower() switch
            {
                "policy-reminder" => new EmailTemplate
                {
                    Subject = "Poliçe Yenileme Hatırlatması - {PolicyNumber}",
                    Body = GetPolicyReminderTemplate()
                },
                "claim-notification" => new EmailTemplate
                {
                    Subject = "Hasar Bildirimi Güncellemesi - {ClaimNumber}",
                    Body = GetClaimNotificationTemplate()
                },
                _ => throw new ArgumentException($"Template '{templateName}' not found")
            };
        }

        private RenderedTemplate RenderTemplate(EmailTemplate template, object model)
        {
            // Simple template rendering - replace placeholders with model properties
            var subject = template.Subject;
            var body = template.Body;

            var properties = model.GetType().GetProperties();
            foreach (var prop in properties)
            {
                var value = prop.GetValue(model)?.ToString() ?? "";
                subject = subject.Replace("{" + prop.Name + "}", value);
                body = body.Replace("{" + prop.Name + "}", value);
            }

            return new RenderedTemplate { Subject = subject, Body = body };
        }

        private string GetPolicyReminderTemplate()
        {
            return @"
<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; margin: 0; padding: 20px; }
        .header { background-color: #2c3e50; color: white; padding: 20px; text-align: center; }
        .content { padding: 20px; }
        .footer { background-color: #ecf0f1; padding: 15px; text-align: center; font-size: 12px; }
        .highlight { background-color: #e74c3c; color: white; padding: 10px; border-radius: 5px; }
    </style>
</head>
<body>
    <div class='header'>
        <h1>{AgencyName}</h1>
        <h2>Poliçe Yenileme Hatırlatması</h2>
    </div>
    <div class='content'>
        <p>Sayın {CustomerName},</p>
        
        <p>Poliçenizin süresi yakında dolacağını bildirmek isteriz:</p>
        
        <div class='highlight'>
            <strong>Poliçe No:</strong> {PolicyNumber}<br>
            <strong>Bitiş Tarihi:</strong> {ExpiryDate}<br>
            <strong>Kalan Süre:</strong> {DaysToExpiry} gün
        </div>
        
        <p>Poliçenizi yenilemek için lütfen en kısa sürede bizimle iletişime geçiniz.</p>
        
        <p>Saygılarımızla,<br>{AgencyName}</p>
    </div>
    <div class='footer'>
        <p>İletişim: {ContactPhone} | {ContactEmail}</p>
    </div>
</body>
</html>";
        }

        private string GetClaimNotificationTemplate()
        {
            return @"
<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; margin: 0; padding: 20px; }
        .header { background-color: #2c3e50; color: white; padding: 20px; text-align: center; }
        .content { padding: 20px; }
        .footer { background-color: #ecf0f1; padding: 15px; text-align: center; font-size: 12px; }
        .status { background-color: #3498db; color: white; padding: 10px; border-radius: 5px; }
    </style>
</head>
<body>
    <div class='header'>
        <h1>{AgencyName}</h1>
        <h2>Hasar Bildirimi Güncellemesi</h2>
    </div>
    <div class='content'>
        <p>Sayın {CustomerName},</p>
        
        <p>Hasar bildiriminizle ilgili güncelleme:</p>
        
        <div class='status'>
            <strong>Hasar No:</strong> {ClaimNumber}<br>
            <strong>Durum:</strong> {Status}<br>
            <strong>Tarih:</strong> {StatusDate}
        </div>
        
        <p>Herhangi bir sorunuz olursa lütfen bizimle iletişime geçiniz.</p>
        
        <p>Saygılarımızla,<br>{AgencyName}</p>
    </div>
    <div class='footer'>
        <p>İletişim: {ContactPhone} | {ContactEmail}</p>
    </div>
</body>
</html>";
        }
    }

    public class EmailSettings
    {
        public string SmtpHost { get; set; } = "localhost";
        public int SmtpPort { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
        public string AgencyName { get; set; } = "Insurance Agency";
        public string ContactPhone { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
    }

    public class EmailTemplate
    {
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
    }

    public class RenderedTemplate
    {
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
    }
}