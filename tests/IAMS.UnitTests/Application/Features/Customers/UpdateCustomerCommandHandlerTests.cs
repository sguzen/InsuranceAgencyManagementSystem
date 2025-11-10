using AutoMapper;
using FluentAssertions;
using FluentValidation;
using IAMS.Application.DTOs.Customer;
using IAMS.Application.Features.Customers.Commands.UpdateCustomer;
using IAMS.Application.Interfaces;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Domain.Entities;
using IAMS.Domain.Enums;
using IAMS.MultiTenancy.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace IAMS.UnitTests.Application.Features.Customers
{
    public class UpdateCustomerCommandHandlerTests
    {
        private readonly Mock<ICustomerRepository> _customerRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IValidator<CreateOrUpdateCustomerDto>> _validatorMock;
        private readonly Mock<ICurrentTenantService> _currentTenantServiceMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly Mock<ILogger<UpdateCustomerCommandHandler>> _loggerMock;
        private readonly UpdateCustomerCommandHandler _handler;

        public UpdateCustomerCommandHandlerTests()
        {
            _customerRepositoryMock = new Mock<ICustomerRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _validatorMock = new Mock<IValidator<CreateOrUpdateCustomerDto>>();
            _currentTenantServiceMock = new Mock<ICurrentTenantService>();
            _currentUserServiceMock = new Mock<ICurrentUserService>();
            _loggerMock = new Mock<ILogger<UpdateCustomerCommandHandler>>();

            _unitOfWorkMock.Setup(x => x.Customers).Returns(_customerRepositoryMock.Object);

            _handler = new UpdateCustomerCommandHandler(
                _unitOfWorkMock.Object,
                _mapperMock.Object,
                _validatorMock.Object,
                _currentTenantServiceMock.Object,
                _currentUserServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_WithValidCommand_ShouldUpdateCustomerWithCurrentUser()
        {
            // Arrange
            var customerId = 1;
            var currentUserName = "admin@example.com";
            _currentUserServiceMock.Setup(x => x.UserName).Returns(currentUserName);

            var existingCustomer = new Customer
            {
                Id = customerId,
                NationalIdNumber = "12345678901",
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                PhoneNumber = "555-1234",
                Status = CustomerStatus.Active,
                CreatedBy = "System",
                CreatedOn = DateTime.UtcNow.AddDays(-30)
            };

            var customerDto = new CustomerDto
            {
                Id = customerId,
                FirstName = "John",
                LastName = "Smith",
                Email = "john.smith@example.com",
                Status = CustomerStatus.Active
            };

            _customerRepositoryMock.Setup(x => x.GetByIdAsync(customerId))
                .ReturnsAsync(existingCustomer);
            _mapperMock.Setup(x => x.Map<CustomerDto>(It.IsAny<Customer>()))
                .Returns(customerDto);
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var command = new UpdateCustomerCommand(customerId, new UpdateCustomerDto
            {
                NationalIdNumber = "12345678901",
                FirstName = "John",
                LastName = "Smith", // Changed last name
                Email = "john.smith@example.com", // Changed email
                PhoneNumber = "555-9999", // Changed phone
                Status = CustomerStatus.Active
            });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.LastName.Should().Be("Smith");
            result.Data.Email.Should().Be("john.smith@example.com");

            existingCustomer.ModifiedBy.Should().Be(currentUserName);
            existingCustomer.ModifiedOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenCurrentUserIsNull_ShouldUseSystemAsModifiedBy()
        {
            // Arrange
            var customerId = 1;
            _currentUserServiceMock.Setup(x => x.UserName).Returns((string?)null);

            var existingCustomer = new Customer
            {
                Id = customerId,
                NationalIdNumber = "12345678901",
                FirstName = "Jane",
                LastName = "Doe",
                Email = "jane.doe@example.com",
                Status = CustomerStatus.Active,
                CreatedBy = "System",
                CreatedOn = DateTime.UtcNow.AddDays(-10)
            };

            var customerDto = new CustomerDto
            {
                Id = customerId,
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@example.com",
                Status = CustomerStatus.Active
            };

            _customerRepositoryMock.Setup(x => x.GetByIdAsync(customerId))
                .ReturnsAsync(existingCustomer);
            _mapperMock.Setup(x => x.Map<CustomerDto>(It.IsAny<Customer>()))
                .Returns(customerDto);
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var command = new UpdateCustomerCommand(customerId, new UpdateCustomerDto
            {
                NationalIdNumber = "12345678901",
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@example.com",
                Status = CustomerStatus.Active
            });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            existingCustomer.ModifiedBy.Should().Be("System");
        }

        [Fact]
        public async Task Handle_WithNonExistentCustomer_ShouldReturnNotFound()
        {
            // Arrange
            var customerId = 999;
            _customerRepositoryMock.Setup(x => x.GetByIdAsync(customerId))
                .ReturnsAsync((Customer?)null);

            var command = new UpdateCustomerCommand(customerId, new UpdateCustomerDto
            {
                NationalIdNumber = "12345678901",
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Status = CustomerStatus.Active
            });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("Müşteri bulunamadı");

            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithDeletedCustomer_ShouldReturnNotFound()
        {
            // Arrange
            var customerId = 1;
            var deletedCustomer = new Customer
            {
                Id = customerId,
                NationalIdNumber = "12345678901",
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Status = CustomerStatus.Active,
                IsDeleted = true // Soft deleted
            };

            _customerRepositoryMock.Setup(x => x.GetByIdAsync(customerId))
                .ReturnsAsync(deletedCustomer);

            var command = new UpdateCustomerCommand(customerId, new UpdateCustomerDto
            {
                NationalIdNumber = "12345678901",
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Status = CustomerStatus.Active
            });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("Müşteri bulunamadı");
        }

        [Fact]
        public async Task Handle_WithDuplicateEmail_ShouldReturnValidationError()
        {
            // Arrange
            var customerId = 1;
            var newEmail = "existing@example.com";

            var existingCustomer = new Customer
            {
                Id = customerId,
                NationalIdNumber = "12345678901",
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Status = CustomerStatus.Active
            };

            var customerWithSameEmail = new Customer
            {
                Id = 2, // Different customer
                Email = newEmail
            };

            _customerRepositoryMock.Setup(x => x.GetByIdAsync(customerId))
                .ReturnsAsync(existingCustomer);
            _customerRepositoryMock.Setup(x => x.GetByEmailAsync(newEmail))
                .ReturnsAsync(customerWithSameEmail);

            var command = new UpdateCustomerCommand(customerId, new UpdateCustomerDto
            {
                NationalIdNumber = "12345678901",
                FirstName = "John",
                LastName = "Doe",
                Email = newEmail,
                Status = CustomerStatus.Active
            });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("Bu e-posta adresi başka bir müşteri tarafından kullanılıyor");

            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenSaveChangesFails_ShouldReturnInternalError()
        {
            // Arrange
            var customerId = 1;
            _currentUserServiceMock.Setup(x => x.UserName).Returns("test.user");

            var existingCustomer = new Customer
            {
                Id = customerId,
                NationalIdNumber = "12345678901",
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Status = CustomerStatus.Active
            };

            _customerRepositoryMock.Setup(x => x.GetByIdAsync(customerId))
                .ReturnsAsync(existingCustomer);
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            var command = new UpdateCustomerCommand(customerId, new UpdateCustomerDto
            {
                NationalIdNumber = "12345678901",
                FirstName = "John",
                LastName = "Smith",
                Email = "john.doe@example.com",
                Status = CustomerStatus.Active
            });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("Müşteri güncellenirken bir hata oluştu");
        }
    }
}
