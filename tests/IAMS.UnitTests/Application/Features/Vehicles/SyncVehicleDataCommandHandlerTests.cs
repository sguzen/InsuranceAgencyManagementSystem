using FluentAssertions;
using IAMS.Application.Features.Vehicles.Commands.SyncVehicleData;
using IAMS.Application.Interfaces;
using IAMS.Domain.Entities;
using IAMS.Shared.DTOs.Vehicle;
using IAMS.Shared.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace IAMS.UnitTests.Application.Features.Vehicles
{
    public class SyncVehicleDataCommandHandlerTests
    {
        private readonly Mock<IVehicleDataService> _vehicleDataServiceMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IVehicleBrandRepository> _vehicleBrandRepositoryMock;
        private readonly Mock<IVehicleModelRepository> _vehicleModelRepositoryMock;
        private readonly Mock<ILogger<SyncVehicleDataCommandHandler>> _loggerMock;
        private readonly SyncVehicleDataCommandHandler _handler;

        public SyncVehicleDataCommandHandlerTests()
        {
            _vehicleDataServiceMock = new Mock<IVehicleDataService>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _vehicleBrandRepositoryMock = new Mock<IVehicleBrandRepository>();
            _vehicleModelRepositoryMock = new Mock<IVehicleModelRepository>();
            _loggerMock = new Mock<ILogger<SyncVehicleDataCommandHandler>>();

            _unitOfWorkMock.Setup(x => x.VehicleBrands).Returns(_vehicleBrandRepositoryMock.Object);
            _unitOfWorkMock.Setup(x => x.VehicleModels).Returns(_vehicleModelRepositoryMock.Object);

            _handler = new SyncVehicleDataCommandHandler(
                _vehicleDataServiceMock.Object,
                _unitOfWorkMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_WhenNoDataFromExternalApi_ShouldReturnSuccessWithWarning()
        {
            // Arrange
            _vehicleDataServiceMock
                .Setup(x => x.FetchVehicleDataAsync())
                .ReturnsAsync(new List<ExternalVehicleDataDto>());

            var command = new SyncVehicleDataCommand();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Warnings.Should().Contain("No data received from external API");
            result.Data.BrandsCreated.Should().Be(0);
            result.Data.ModelsCreated.Should().Be(0);
        }

        [Fact]
        public async Task Handle_WithNewBrandAndModel_ShouldCreateBoth()
        {
            // Arrange
            var externalData = new List<ExternalVehicleDataDto>
            {
                new ExternalVehicleDataDto
                {
                    Id = 1,
                    BrandCode = 100,
                    BrandName = "Toyota",
                    ModelName = "Corolla",
                    BrandModelName = "Toyota Corolla"
                }
            };

            _vehicleDataServiceMock
                .Setup(x => x.FetchVehicleDataAsync())
                .ReturnsAsync(externalData);

            _vehicleBrandRepositoryMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<VehicleBrand>());

            _vehicleModelRepositoryMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<VehicleModel>());

            var brandIdCounter = 0;
            _vehicleBrandRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<VehicleBrand>()))
                .ReturnsAsync((VehicleBrand b) => { b.Id = ++brandIdCounter; return b; });

            var modelIdCounter = 0;
            _vehicleModelRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<VehicleModel>()))
                .ReturnsAsync((VehicleModel m) => { m.Id = ++modelIdCounter; return m; });

            _unitOfWorkMock
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var command = new SyncVehicleDataCommand { UpdateExisting = true };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.BrandsCreated.Should().Be(1);
            result.Data.ModelsCreated.Should().Be(1);

            _vehicleBrandRepositoryMock.Verify(x => x.AddAsync(It.IsAny<VehicleBrand>()), Times.Once);
            _vehicleModelRepositoryMock.Verify(x => x.AddAsync(It.IsAny<VehicleModel>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithExistingBrand_WhenUpdateExistingIsTrue_ShouldUpdateBrand()
        {
            // Arrange
            var externalData = new List<ExternalVehicleDataDto>
            {
                new ExternalVehicleDataDto
                {
                    Id = 1,
                    BrandCode = 100,
                    BrandName = "Toyota Updated",
                    ModelName = "Corolla",
                    BrandModelName = "Toyota Corolla"
                }
            };

            var existingBrand = new VehicleBrand
            {
                Id = 1,
                Code = "100",
                Name = "Toyota",
                IsActive = true
            };

            _vehicleDataServiceMock
                .Setup(x => x.FetchVehicleDataAsync())
                .ReturnsAsync(externalData);

            _vehicleBrandRepositoryMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<VehicleBrand> { existingBrand });

            _vehicleModelRepositoryMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<VehicleModel>());

            var modelIdCounter = 0;
            _vehicleModelRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<VehicleModel>()))
                .ReturnsAsync((VehicleModel m) => { m.Id = ++modelIdCounter; return m; });

            _unitOfWorkMock
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var command = new SyncVehicleDataCommand { UpdateExisting = true };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data!.BrandsUpdated.Should().Be(1);
            existingBrand.Name.Should().Be("Toyota Updated");
            _vehicleBrandRepositoryMock.Verify(x => x.Update(It.IsAny<VehicleBrand>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithExistingBrand_WhenUpdateExistingIsFalse_ShouldNotUpdateBrand()
        {
            // Arrange
            var externalData = new List<ExternalVehicleDataDto>
            {
                new ExternalVehicleDataDto
                {
                    Id = 1,
                    BrandCode = 100,
                    BrandName = "Toyota Updated",
                    ModelName = "Corolla",
                    BrandModelName = "Toyota Corolla"
                }
            };

            var existingBrand = new VehicleBrand
            {
                Id = 1,
                Code = "100",
                Name = "Toyota",
                IsActive = true
            };

            _vehicleDataServiceMock
                .Setup(x => x.FetchVehicleDataAsync())
                .ReturnsAsync(externalData);

            _vehicleBrandRepositoryMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<VehicleBrand> { existingBrand });

            _vehicleModelRepositoryMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<VehicleModel>());

            var modelIdCounter = 0;
            _vehicleModelRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<VehicleModel>()))
                .ReturnsAsync((VehicleModel m) => { m.Id = ++modelIdCounter; return m; });

            _unitOfWorkMock
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var command = new SyncVehicleDataCommand { UpdateExisting = false };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data!.BrandsUpdated.Should().Be(0);
            existingBrand.Name.Should().Be("Toyota"); // Should not be updated
            _vehicleBrandRepositoryMock.Verify(x => x.Update(It.IsAny<VehicleBrand>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenDeactivateMissingIsTrue_ShouldDeactivateMissingBrands()
        {
            // Arrange
            var externalData = new List<ExternalVehicleDataDto>
            {
                new ExternalVehicleDataDto
                {
                    Id = 1,
                    BrandCode = 100,
                    BrandName = "Toyota",
                    ModelName = "Corolla",
                    BrandModelName = "Toyota Corolla"
                }
            };

            var existingBrands = new List<VehicleBrand>
            {
                new VehicleBrand { Id = 1, Code = "100", Name = "Toyota", IsActive = true },
                new VehicleBrand { Id = 2, Code = "200", Name = "Honda", IsActive = true }
            };

            _vehicleDataServiceMock
                .Setup(x => x.FetchVehicleDataAsync())
                .ReturnsAsync(externalData);

            _vehicleBrandRepositoryMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(existingBrands);

            _vehicleModelRepositoryMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<VehicleModel>());

            var brandIdCounter = 0;
            _vehicleBrandRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<VehicleBrand>()))
                .ReturnsAsync((VehicleBrand b) => { b.Id = ++brandIdCounter; return b; });

            var modelIdCounter = 0;
            _vehicleModelRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<VehicleModel>()))
                .ReturnsAsync((VehicleModel m) => { m.Id = ++modelIdCounter; return m; });

            _unitOfWorkMock
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var command = new SyncVehicleDataCommand
            {
                UpdateExisting = true,
                DeactivateMissing = true
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data!.Warnings.Should().Contain(w => w.Contains("Honda"));
            existingBrands[1].IsActive.Should().BeFalse(); // Honda should be deactivated
            _vehicleBrandRepositoryMock.Verify(
                x => x.Update(It.Is<VehicleBrand>(b => b.Code == "200" && !b.IsActive)),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WhenExternalApiFails_ShouldReturnFailure()
        {
            // Arrange
            _vehicleDataServiceMock
                .Setup(x => x.FetchVehicleDataAsync())
                .ThrowsAsync(new HttpRequestException("API is unavailable"));

            var command = new SyncVehicleDataCommand();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Data!.Success.Should().BeFalse();
            result.Data.ErrorMessage.Should().Contain("API is unavailable");
        }

        [Fact]
        public async Task Handle_WithMultipleBrandsAndModels_ShouldCreateAll()
        {
            // Arrange
            var externalData = new List<ExternalVehicleDataDto>
            {
                new ExternalVehicleDataDto
                {
                    Id = 1,
                    BrandCode = 100,
                    BrandName = "Toyota",
                    ModelName = "Corolla",
                    BrandModelName = "Toyota Corolla"
                },
                new ExternalVehicleDataDto
                {
                    Id = 2,
                    BrandCode = 100,
                    BrandName = "Toyota",
                    ModelName = "Camry",
                    BrandModelName = "Toyota Camry"
                },
                new ExternalVehicleDataDto
                {
                    Id = 3,
                    BrandCode = 200,
                    BrandName = "Honda",
                    ModelName = "Civic",
                    BrandModelName = "Honda Civic"
                }
            };

            _vehicleDataServiceMock
                .Setup(x => x.FetchVehicleDataAsync())
                .ReturnsAsync(externalData);

            _vehicleBrandRepositoryMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<VehicleBrand>());

            _vehicleModelRepositoryMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<VehicleModel>());

            var brandIdCounter = 0;
            _vehicleBrandRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<VehicleBrand>()))
                .ReturnsAsync((VehicleBrand b) => { b.Id = ++brandIdCounter; return b; });

            var modelIdCounter = 0;
            _vehicleModelRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<VehicleModel>()))
                .ReturnsAsync((VehicleModel m) => { m.Id = ++modelIdCounter; return m; });

            _unitOfWorkMock
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var command = new SyncVehicleDataCommand();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data!.BrandsCreated.Should().Be(2); // Toyota and Honda
            result.Data.ModelsCreated.Should().Be(3); // Corolla, Camry, Civic
            _vehicleBrandRepositoryMock.Verify(x => x.AddAsync(It.IsAny<VehicleBrand>()), Times.Exactly(2));
            _vehicleModelRepositoryMock.Verify(x => x.AddAsync(It.IsAny<VehicleModel>()), Times.Exactly(3));
        }
    }
}
