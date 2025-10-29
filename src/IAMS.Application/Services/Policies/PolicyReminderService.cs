using IAMS.Application.Interfaces.Repositories;
using IAMS.Domain.Entities;
using IAMS.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Services.Policies
{
    public class PolicyReminderService : IPolicyReminderService
    {
        private readonly IPolicyRepository _policyRepository;
        private readonly IEmailService _emailService;
        private readonly ILogger<PolicyReminderService> _logger;

        public PolicyReminderService(
            IPolicyRepository policyRepository,
            IEmailService emailService,
            ILogger<PolicyReminderService> logger)
        {
            _policyRepository = policyRepository;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<IEnumerable<Policy>> GetPoliciesForReminderAsync(DateTime reminderDate, int daysBefore = 30)
        {
            var expiryDate = reminderDate.AddDays(daysBefore);
            return await _policyRepository.GetPoliciesExpiringInDateRangeAsync(expiryDate, reminderDate);
        }

        public async Task<IEnumerable<Policy>> GetExpiringPoliciesAsync(int daysBefore = 30)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(daysBefore);
            return await _policyRepository.GetPoliciesExpiringInDateRangeAsync(cutoffDate, DateTime.UtcNow.Date);
        }

        public async Task<IEnumerable<Policy>> GetOverduePoliciesAsync()
        {
            return await _policyRepository.GetExpiredPoliciesAsync();
        }

        public async Task SendPolicyReminderAsync(Policy policy, int daysBefore)
        {
            try
            {
                var message = new EmailMessage
                {
                    ToEmail = policy.Customer.Email,
                    ToName = $"{policy.Customer.FirstName} {policy.Customer.LastName}",
                    Subject = $"Poliçe Yenileme Hatırlatması - {policy.PolicyNumber}",
                    Body = $@"
                Sayın {policy.Customer.FirstName} {policy.Customer.LastName},
                
                {policy.PolicyNumber} numaralı poliçeniz {daysBefore} gün içinde sona erecektir.
                
                Bitiş Tarihi: {policy.EndDate:dd.MM.yyyy}
                Sigorta Şirketi: {policy.InsuranceCompany.Name}
                Prim Tutarı: {policy.PremiumAmount:C}
                
                Lütfen yenileme için bizimle iletişime geçiniz.
            "
                };

                var success = await _emailService.SendAsync(message);

                if (success)
                {
                    _logger.LogInformation("Reminder sent for policy {PolicyNumber}", policy.PolicyNumber);
                }
                else
                {
                    _logger.LogWarning("Failed to send reminder for policy {PolicyNumber}", policy.PolicyNumber);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending reminder for policy {PolicyNumber}", policy.PolicyNumber);
                throw;
            }
        }

        public async Task SendBatchRemindersAsync(IEnumerable<Policy> policies, int daysBefore)
        {
            var tasks = policies.Select(p => SendPolicyReminderAsync(p, daysBefore));
            await Task.WhenAll(tasks);

            _logger.LogInformation("Sent {Count} policy reminders", policies.Count());
        }

        public async Task ProcessExpiringPoliciesAsync()
        {
            _logger.LogInformation("Processing expiring policies");

            var result = await _policyRepository.GetExpiringPoliciesAsync(30);

            if (!result.Any())
            {
                _logger.LogWarning("No expiring policies found");
                return;
            }

            foreach (var policy in result)
            {
                try
                {
                    await SendPolicyReminderAsync(policy,0);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send reminder for policy {PolicyNumber}", policy.PolicyNumber);
                }
            }
        }
    }
}