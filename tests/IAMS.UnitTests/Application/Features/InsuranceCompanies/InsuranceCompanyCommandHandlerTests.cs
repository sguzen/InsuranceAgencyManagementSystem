using AutoMapper;
using FluentAssertions;
using IAMS.Application.DTOs.InsuranceCompany;
using IAMS.Application.Features.InsuranceCompanies.Commands.CreateInsuranceCompany;
using IAMS.Application.Features.InsuranceCompanies.Commands.UpdateInsuranceCompany;
using IAMS.Application.Features.InsuranceCompanies.Commands.DeleteInsuranceCompany;
using IAMS.Application.Interfaces;
using IAMS.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using IAMS.Shared.Interfaces.Repositories;

namespace IAMS.UnitTests.Application.Features.InsuranceCompanies
{
    public class InsuranceCompanyCommandHandlerTests
    {
        #region CreateInsuranceCompanyCommandHandler Tests

        [Fact]
        public async Task CreateInsuranceCompany_WithValidData_ShouldCreateWithCurrentUser()
        {
            // Arrange
            var currentUserName = "admin@example.com";
            var repositoryMock = new Mock<IInsuranceCompanyRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var mapperMock = new Mock<IMapper>();
            var currentUserServiceMock = new Mock<ICurrentUserService>();
            var loggerMock = new Mock<ILogger<CreateInsuranceCompanyCommandHandler>>();

            currentUserServiceMock.Setup(x => x.UserName).Returns(currentUserName);
            unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var companyDto = new InsuranceCompanyDto
            {
                Id = 1,
                Name = "AXA Sigorta",
                Code = "AXA1234567",
                Description = "Leading insurance company",
                ContactEmail = "info@axa.com",
                ContactPhone = "555-1234",
                Address = "123 Main St",
                Website = "www.axa.com",
                IsActive = true
            };

            mapperMock.Setup(x => x.Map<InsuranceCompanyDto>(It.IsAny<InsuranceCompany>()))
                .Returns(companyDto);

            var handler = new CreateInsuranceCompanyCommandHandler(
                repositoryMock.Object,
                unitOfWorkMock.Object,
                mapperMock.Object,
                currentUserServiceMock.Object,
                loggerMock.Object);

            var command = new CreateInsuranceCompanyCommand(new CreateInsuranceCompanyDto
            {
                Name = "AXA Sigorta",
                Description = "Leading insurance company",
                ContactEmail = "info@axa.com",
                ContactPhone = "555-1234",
                Address = "123 Main St",
                Website = "www.axa.com"
            });

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Name.Should().Be("AXA Sigorta");
            result.Data.Code.Should().NotBeNullOrEmpty();
            result.Data.Code.Length.Should().Be(10);

            repositoryMock.Verify(x => x.AddAsync(It.Is<InsuranceCompany>(c =>
                c.Name == "AXA Sigorta" &&
                c.CreatedBy == currentUserName &&
                c.IsActive == true
            )), Times.Once);
        }

        [Fact]
        public async Task CreateInsuranceCompany_WhenCurrentUserIsNull_ShouldUseSystem()
        {
            // Arrange
            var repositoryMock = new Mock<IInsuranceCompanyRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var mapperMock = new Mock<IMapper>();
            var currentUserServiceMock = new Mock<ICurrentUserService>();
            var loggerMock = new Mock<ILogger<CreateInsuranceCompanyCommandHandler>>();

            currentUserServiceMock.Setup(x => x.UserName).Returns((string?)null);
            unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var companyDto = new InsuranceCompanyDto
            {
                Id = 1,
                Name = "Test Insurance",
                Code = "TES1234567",
                IsActive = true
            };

            mapperMock.Setup(x => x.Map<InsuranceCompanyDto>(It.IsAny<InsuranceCompany>()))
                .Returns(companyDto);

            var handler = new CreateInsuranceCompanyCommandHandler(
                repositoryMock.Object,
                unitOfWorkMock.Object,
                mapperMock.Object,
                currentUserServiceMock.Object,
                loggerMock.Object);

            var command = new CreateInsuranceCompanyCommand(new CreateInsuranceCompanyDto
            {
                Name = "Test Insurance"
            });

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            repositoryMock.Verify(x => x.AddAsync(It.Is<InsuranceCompany>(c =>
                c.CreatedBy == "System"
            )), Times.Once);
        }

        #endregion

        #region UpdateInsuranceCompanyCommandHandler Tests

        [Fact]
        public async Task UpdateInsuranceCompany_WithValidData_ShouldUpdateWithCurrentUser()
        {
            // Arrange
            var companyId = 1;
            var currentUserName = "admin@example.com";
            var repositoryMock = new Mock<IInsuranceCompanyRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var mapperMock = new Mock<IMapper>();
            var currentUserServiceMock = new Mock<ICurrentUserService>();
            var loggerMock = new Mock<ILogger<UpdateInsuranceCompanyCommandHandler>>();

            var existingCompany = InsuranceCompany.Create(
                "AXA Sigorta",
                "Old description",
                "old@axa.com",
                "555-0000",
                "Old Address",
                "www.old-axa.com",
                "System");
            existingCompany.Id = companyId;

            var companyDto = new InsuranceCompanyDto
            {
                Id = companyId,
                Name = "AXA Sigorta Updated",
                Code = existingCompany.Code,
                Description = "New description",
                ContactEmail = "new@axa.com",
                ContactPhone = "555-9999",
                Address = "New Address",
                Website = "www.new-axa.com",
                IsActive = true
            };

            repositoryMock.Setup(x => x.GetByIdAsync(companyId))
                .ReturnsAsync(existingCompany);
            mapperMock.Setup(x => x.Map<InsuranceCompanyDto>(It.IsAny<InsuranceCompany>()))
                .Returns(companyDto);
            currentUserServiceMock.Setup(x => x.UserName).Returns(currentUserName);
            unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var handler = new UpdateInsuranceCompanyCommandHandler(
                repositoryMock.Object,
                unitOfWorkMock.Object,
                mapperMock.Object,
                currentUserServiceMock.Object,
                loggerMock.Object);

            var command = new UpdateInsuranceCompanyCommand(companyId, new UpdateInsuranceCompanyDto
            {
                Name = "AXA Sigorta Updated",
                Description = "New description",
                ContactEmail = "new@axa.com",
                ContactPhone = "555-9999",
                Address = "New Address",
                Website = "www.new-axa.com",
                IsActive = true
            });

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            existingCompany.ModifiedBy.Should().Be(currentUserName);
            existingCompany.ModifiedOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            existingCompany.Name.Should().Be("AXA Sigorta Updated");
            existingCompany.Description.Should().Be("New description");
        }

        [Fact]
        public async Task UpdateInsuranceCompany_WithNonExistentCompany_ShouldReturnNotFound()
        {
            // Arrange
            var companyId = 999;
            var repositoryMock = new Mock<IInsuranceCompanyRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var mapperMock = new Mock<IMapper>();
            var currentUserServiceMock = new Mock<ICurrentUserService>();
            var loggerMock = new Mock<ILogger<UpdateInsuranceCompanyCommandHandler>>();

            repositoryMock.Setup(x => x.GetByIdAsync(companyId))
                .ReturnsAsync((InsuranceCompany?)null);

            var handler = new UpdateInsuranceCompanyCommandHandler(
                repositoryMock.Object,
                unitOfWorkMock.Object,
                mapperMock.Object,
                currentUserServiceMock.Object,
                loggerMock.Object);

            var command = new UpdateInsuranceCompanyCommand(companyId, new UpdateInsuranceCompanyDto
            {
                Name = "Test"
            });

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("Sigorta şirketi bulunamadı");
            unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region DeleteInsuranceCompanyCommandHandler Tests

        [Fact]
        public async Task DeleteInsuranceCompany_WithValidId_ShouldSoftDeleteWithCurrentUser()
        {
            // Arrange
            var companyId = 1;
            var currentUserName = "admin@example.com";
            var repositoryMock = new Mock<IInsuranceCompanyRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var currentUserServiceMock = new Mock<ICurrentUserService>();
            var loggerMock = new Mock<ILogger<DeleteInsuranceCompanyCommandHandler>>();

            var existingCompany = InsuranceCompany.Create(
                "Test Insurance",
                null,
                null,
                null,
                null,
                null,
                "System");
            existingCompany.Id = companyId;

            repositoryMock.Setup(x => x.GetByIdAsync(companyId))
                .ReturnsAsync(existingCompany);
            currentUserServiceMock.Setup(x => x.UserName).Returns(currentUserName);
            unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var handler = new DeleteInsuranceCompanyCommandHandler(
                repositoryMock.Object,
                unitOfWorkMock.Object,
                currentUserServiceMock.Object,
                loggerMock.Object);

            var command = new DeleteInsuranceCompanyCommand(companyId);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            existingCompany.IsDeleted.Should().BeTrue();
            existingCompany.DeletedBy.Should().Be(currentUserName);
            existingCompany.DeletedOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

            unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteInsuranceCompany_WhenCurrentUserIsNull_ShouldUseSystem()
        {
            // Arrange
            var companyId = 1;
            var repositoryMock = new Mock<IInsuranceCompanyRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var currentUserServiceMock = new Mock<ICurrentUserService>();
            var loggerMock = new Mock<ILogger<DeleteInsuranceCompanyCommandHandler>>();

            var existingCompany = InsuranceCompany.Create("Test", null, null, null, null, null, "System");
            existingCompany.Id = companyId;

            repositoryMock.Setup(x => x.GetByIdAsync(companyId))
                .ReturnsAsync(existingCompany);
            currentUserServiceMock.Setup(x => x.UserName).Returns((string?)null);
            unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var handler = new DeleteInsuranceCompanyCommandHandler(
                repositoryMock.Object,
                unitOfWorkMock.Object,
                currentUserServiceMock.Object,
                loggerMock.Object);

            var command = new DeleteInsuranceCompanyCommand(companyId);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            existingCompany.DeletedBy.Should().Be("System");
        }

        [Fact]
        public async Task DeleteInsuranceCompany_WithNonExistentCompany_ShouldReturnNotFound()
        {
            // Arrange
            var companyId = 999;
            var repositoryMock = new Mock<IInsuranceCompanyRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var currentUserServiceMock = new Mock<ICurrentUserService>();
            var loggerMock = new Mock<ILogger<DeleteInsuranceCompanyCommandHandler>>();

            repositoryMock.Setup(x => x.GetByIdAsync(companyId))
                .ReturnsAsync((InsuranceCompany?)null);

            var handler = new DeleteInsuranceCompanyCommandHandler(
                repositoryMock.Object,
                unitOfWorkMock.Object,
                currentUserServiceMock.Object,
                loggerMock.Object);

            var command = new DeleteInsuranceCompanyCommand(companyId);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("Sigorta şirketi bulunamadı");
            unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DeleteInsuranceCompany_WithActivePolicies_ShouldReturnValidationError()
        {
            // Arrange
            var companyId = 1;
            var repositoryMock = new Mock<IInsuranceCompanyRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var currentUserServiceMock = new Mock<ICurrentUserService>();
            var loggerMock = new Mock<ILogger<DeleteInsuranceCompanyCommandHandler>>();

            var existingCompany = InsuranceCompany.Create("Test", null, null, null, null, null, "System");
            existingCompany.Id = companyId;

            // Add an active policy to the company
            existingCompany.Policies = new List<Policy>
            {
                new Policy
                {
                    Id = 1,
                    Status = IAMS.Domain.Enums.PolicyStatus.Active,
                    IsDeleted = false
                }
            };

            repositoryMock.Setup(x => x.GetByIdAsync(companyId))
                .ReturnsAsync(existingCompany);

            var handler = new DeleteInsuranceCompanyCommandHandler(
                repositoryMock.Object,
                unitOfWorkMock.Object,
                currentUserServiceMock.Object,
                loggerMock.Object);

            var command = new DeleteInsuranceCompanyCommand(companyId);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("aktif poliçeleri olduğu için silinemez");
            unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion
    }
}
