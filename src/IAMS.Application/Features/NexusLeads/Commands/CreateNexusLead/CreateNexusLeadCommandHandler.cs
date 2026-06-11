using IAMS.Application.Services;
using IAMS.Domain.Entities;
using IAMS.Domain.Enums;
using IAMS.Shared.DTOs.Customer;
using IAMS.Shared.DTOs.NexusLead;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.NexusLeads.Commands.CreateNexusLead
{
    public class CreateNexusLeadCommandHandler : IRequestHandler<CreateNexusLeadCommand, Result<CustomerDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICustomerCodeGenerator _customerCodeGenerator;
        private readonly ILogger<CreateNexusLeadCommandHandler> _logger;

        public CreateNexusLeadCommandHandler(
            IUnitOfWork unitOfWork,
            ICustomerCodeGenerator customerCodeGenerator,
            ILogger<CreateNexusLeadCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _customerCodeGenerator = customerCodeGenerator;
            _logger = logger;
        }

        public async Task<Result<CustomerDto>> Handle(CreateNexusLeadCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var extractedData = request.ExtractionResult.ExtractedData;

                if (string.IsNullOrWhiteSpace(extractedData.TrncIdNumber))
                {
                    return Result<CustomerDto>.ValidationFailure(
                        "Kimlik numarasi zorunludur.",
                        new List<string> { "TrncIdNumber is required" });
                }

                var existingCustomer = await _unitOfWork.Customers.GetByIdentificationNoAsync(extractedData.TrncIdNumber);
                if (existingCustomer != null)
                {
                    _logger.LogInformation(
                        "Nexus lead for ID {TrncIdNumber} matched existing customer {CustomerId}.",
                        extractedData.TrncIdNumber, existingCustomer.Id);

                    return Result<CustomerDto>.Failure(
                        "Bu kimlik numarasi ile kayitli musteri zaten mevcut.",
                        new List<string> { $"Customer with ID {existingCustomer.Id} already exists." },
                        statusCode: 409);
                }

                var customer = MapToCustomerEntity(extractedData, request.ExtractionResult.Source);

                customer.CustomerCode = await _customerCodeGenerator.GenerateAsync();
                if (!await _customerCodeGenerator.IsCodeUniqueAsync(customer.CustomerCode))
                {
                    _logger.LogWarning("Generated customer code {Code} is not unique, using fallback.", customer.CustomerCode);
                    customer.CustomerCode = _customerCodeGenerator.GenerateFallbackCode();
                }

                await _unitOfWork.Customers.AddAsync(customer);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Nexus lead successfully mapped to new customer. CustomerId={CustomerId}, Code={CustomerCode}, Source={Source}",
                    customer.Id, customer.CustomerCode, request.ExtractionResult.Source);

                var customerDto = MapToCustomerDto(customer);
                return Result<CustomerDto>.Success(customerDto, "Musteri basariyla kaydedildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while processing incoming Nexus lead.");
                return Result<CustomerDto>.InternalError(
                    "Musteri kaydi sirasinda bir hata olustu.",
                    new List<string> { ex.Message });
            }
        }

        private static Customer MapToCustomerEntity(ExtractedData data, string source)
        {
            var (firstName, lastName) = ParseNames(data);
            var dateOfBirth = ParseDateOfBirth(data.DateOfBirth);
            var gender = ParseGender(data.Gender);
            var identificationNumber = data.TrncIdNumber?.Trim() ?? string.Empty;
            var phone = DetermineBestPhone(data);

            var customer = new Customer
            {
                FirstName = firstName,
                LastName = lastName,
                IdentificationNumber = identificationNumber,
                IdentificationType = IdentificationType.IdCard,
                DateOfBirth = dateOfBirth,
                Gender = gender,
                Email = data.Email?.Trim().ToLower() ?? string.Empty,
                Phone = phone,
                MobilePhoneNumber = data.MobilePhoneNumber?.Trim() ?? phone,
                Address1 = data.Address ?? data.Address1,
                Status = CustomerStatus.Active,
                Type = CustomerType.Individual,
                Notes = $"Kaynak: {source} | Nexus Bot",
                CreatedBy = "NexusBot",
                CreatedOn = DateTime.UtcNow
            };

            return customer;
        }

        private static (string FirstName, string LastName) ParseNames(ExtractedData data)
        {
            if (!string.IsNullOrWhiteSpace(data.FirstName) && !string.IsNullOrWhiteSpace(data.LastName))
            {
                return (data.FirstName.Trim(), data.LastName.Trim());
            }

            if (!string.IsNullOrWhiteSpace(data.FullName))
            {
                var parts = data.FullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    var firstName = string.Join(' ', parts.Take(parts.Length - 1));
                    var lastName = parts.Last();
                    return (firstName.Trim(), lastName.Trim());
                }

                return (parts.FirstOrDefault() ?? "Bilinmiyor", "Bilinmiyor");
            }

            return ("Bilinmiyor", "Bilinmiyor");
        }

        private static DateTime? ParseDateOfBirth(string? dateOfBirthString)
        {
            if (string.IsNullOrWhiteSpace(dateOfBirthString))
                return null;

            var formats = new[]
            {
                "yyyy-MM-dd", "dd/MM/yyyy", "dd.MM.yyyy",
                "yyyy/MM/dd", "MM/dd/yyyy", "dd-MM-yyyy"
            };

            if (DateTime.TryParseExact(dateOfBirthString.Trim(), formats,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out var parsedDate))
            {
                return parsedDate;
            }

            if (DateTime.TryParse(dateOfBirthString, out var genericDate))
            {
                return genericDate;
            }

            return null;
        }

        private static Gender ParseGender(string? genderString)
        {
            if (string.IsNullOrWhiteSpace(genderString))
                return Gender.Male;

            var normalized = genderString.Trim().ToLowerInvariant();
            return normalized switch
            {
                "female" or "f" or "kadin" or "woman" => Gender.Female,
                _ => Gender.Male
            };
        }

        private static string DetermineBestPhone(ExtractedData data)
        {
            if (!string.IsNullOrWhiteSpace(data.Phone))
                return data.Phone.Trim();

            if (!string.IsNullOrWhiteSpace(data.MobilePhoneNumber))
                return data.MobilePhoneNumber.Trim();

            return string.Empty;
        }

        private static CustomerDto MapToCustomerDto(Customer customer)
        {
            return new CustomerDto
            {
                Id = customer.Id,
                CustomerCode = customer.CustomerCode,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                MobilePhoneNumber = customer.MobilePhoneNumber,
                IdentificationNumber = customer.IdentificationNumber,
                DateOfBirth = customer.DateOfBirth,
                Gender = customer.Gender,
                Status = customer.Status,
                Type = customer.Type,
                Address1 = customer.Address1,
                CreatedOn = customer.CreatedOn
            };
        }
    }
}
