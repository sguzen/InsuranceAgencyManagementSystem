using IAMS.Shared.DTOs.Customer;
using IAMS.Shared.DTOs.NexusLead;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.NexusLeads.Commands.CreateNexusLead
{
    public class CreateNexusLeadCommand : IRequest<Result<CustomerDto>>
    {
        public ExtractionResult ExtractionResult { get; set; } = new ExtractionResult();

        public CreateNexusLeadCommand(ExtractionResult extractionResult)
        {
            ExtractionResult = extractionResult;
        }
    }
}
