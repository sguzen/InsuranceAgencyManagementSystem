using AutoMapper;
using FluentAssertions;
using IAMS.Application.DTOs.Customer;
using IAMS.Application.Features.Customers.Commands.CreateCustomer;
using IAMS.Application.Interfaces;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Services;
using IAMS.Domain.Entities;
using IAMS.Domain.Enums;
using IAMS.MultiTenancy.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace IAMS.UnitTests.Application.Features.Customers
{
    public class CreateCustomerCommandHandlerTests
    {
        private readonly Mock<ICustomerRepository> _customerRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ICurrentTenantService> _currentTenantServiceMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly Mock<ICustomerCodeGenerator> _customerCodeGeneratorMock;
        private readonly Mock<ILogger<CreateCustomerCommandHandler>> _loggerMock;
        private readonly CreateCustomerCommandHandler _handler;

        public CreateCustomerCommandHandlerTests()
        {
            _customerRepositoryMock = new Mock<ICustomerRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _currentTenantServiceMock = new Mock<ICurrentTenantService>();
            _currentUserServiceMock = new Mock<ICurrentUserService>();
            _customerCodeGeneratorMock = new Mock<ICustomerCodeGenerator>();
            _loggerMock = new Mock<ILogger<CreateCustomerCommandHandler>>();

            _unitOfWorkMock.Setup(x => x.Customers).Returns(_customerRepositoryMock.Object);
            _customerCodeGeneratorMock.Setup(x => x.GenerateAsync()).ReturnsAsync("CUST-001");

            _handler = new CreateCustomerCommandHandler(
                _unitOfWorkMock.Object,
                _mapperMock.Object,
                _currentTenantServiceMock.Object,
                _currentUserServiceMock.Object,
                _customerCodeGeneratorMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_WithValidCommand_ShouldCreateCustomerWithCurrentUser()
        {
            // Arrange
            var currentUserName = "test.user@example.com";
            _currentUserServiceMock.Setup(x => x.UserName).Returns(currentUserName);
            _customerRepositoryMock.Setup(x => x.GetByIdentificationNoAsync(It.IsAny<string>()))
                .ReturnsAsync((Customer?)null);
            _customerRepositoryMock.Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((Customer?)null);
            _customerRepositoryMock.Setup(x => x.GetByPhoneAsync(It.IsAny<string>()))
                .ReturnsAsync((Customer?)null);
            _customerCodeGeneratorMock.Setup(x => x.IsCodeUniqueAsync(It.IsAny<string>()))
                .ReturnsAsync(true);
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var customerDto = new CustomerDto
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Status = CustomerStatus.Active
            };

            _mapperMock.Setup(x => x.Map<CustomerDto>(It.IsAny<Customer>()))
                .Returns(customerDto);

            var command = new CreateCustomerCommand(new CreateOrUpdateCustomerDto
            {
                IdentificationNumber = "12345678901",
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                HomePhone = "555-1234",
                MobilePhoneNumber = "555-5678",
                DateOfBirth = new DateTime(1990, 1, 1),
                Address1 = "123 Main St",
                Status = CustomerStatus.Active
            });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.FirstName.Should().Be("John");
            result.Data.LastName.Should().Be("Doe");

            _customerRepositoryMock.Verify(x => x.AddAsync(It.Is<Customer>(c =>
                c.FirstName == "John" &&
                c.LastName == "Doe" &&
                c.CreatedBy == currentUserName
            )), Times.Once);

            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenCurrentUserIsNull_ShouldUseSystemAsCreatedBy()
        {
            // Arrange
            _currentUserServiceMock.Setup(x => x.UserName).Returns((string?)null);
            _customerRepositoryMock.Setup(x => x.GetByIdentificationNoAsync(It.IsAny<string>()))
                .ReturnsAsync((Customer?)null);
            _customerRepositoryMock.Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((Customer?)null);
            _customerRepositoryMock.Setup(x => x.GetByPhoneAsync(It.IsAny<string>()))
                .ReturnsAsync((Customer?)null);
            _customerCodeGeneratorMock.Setup(x => x.IsCodeUniqueAsync(It.IsAny<string>()))
                .ReturnsAsync(true);
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var customerDto = new CustomerDto
            {
                Id = 1,
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@example.com",
                Status = CustomerStatus.Active
            };

            _mapperMock.Setup(x => x.Map<CustomerDto>(It.IsAny<Customer>()))
                .Returns(customerDto);

            var command = new CreateCustomerCommand(new CreateOrUpdateCustomerDto
            {
                IdentificationNumber = "12345678901",
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@example.com",
                DateOfBirth = new DateTime(1985, 5, 15),
                Status = CustomerStatus.Active
            });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();

            _customerRepositoryMock.Verify(x => x.AddAsync(It.Is<Customer>(c =>
                c.CreatedBy == "System"
            )), Times.Once);
        }

        [Fact]
        public async Task Handle_WithDuplicateNationalId_ShouldReturnFailure()
        {
            // Arrange
            var nationalId = "12345678901";
            _customerRepositoryMock.Setup(x => x.GetByIdentificationNoAsync(nationalId))
                .ReturnsAsync(new Customer { IdentificationNumber = nationalId });

            var command = new CreateCustomerCommand(new CreateOrUpdateCustomerDto
            {
                IdentificationNumber = nationalId,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                DateOfBirth = new DateTime(1990, 1, 1),
                Status = CustomerStatus.Active
            });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("Bu T.C. Kimlik Numarası ile kayıtlı bir müşteri zaten mevcut");

            _customerRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Customer>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithDuplicateEmail_ShouldReturnFailure()
        {
            // Arrange
            var email = "test@example.com";
            _customerRepositoryMock.Setup(x => x.GetByIdentificationNoAsync(It.IsAny<string>()))
                .ReturnsAsync((Customer?)null);
            _customerRepositoryMock.Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync(new Customer { Email = email });

            var command = new CreateCustomerCommand(new CreateOrUpdateCustomerDto
            {
                IdentificationNumber = "12345678901",
                FirstName = "John",
                LastName = "Doe",
                Email = email,
                DateOfBirth = new DateTime(1990, 1, 1),
                Status = CustomerStatus.Active
            });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("Bu e-posta adresi ile kayıtlı bir müşteri zaten mevcut");

            _customerRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Customer>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenSaveChangesFails_ShouldReturnInternalError()
        {
            // Arrange
            _currentUserServiceMock.Setup(x => x.UserName).Returns("test.user");
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            var command = new CreateCustomerCommand(new CreateOrUpdateCustomerDto
            {
                IdentificationNumber = "12345678901",
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                DateOfBirth = new DateTime(1990, 1, 1),
                Status = CustomerStatus.Active
            });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("Müşteri oluşturulurken bir hata oluştu");
        }
    }
}
