using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LeMarconnes.API.Tests.TestData;
using LeMarconnes.Shared.DTOs;
using Moq;

namespace LeMarconnes.API.Tests.Controllers;

/// <summary>
/// Integration tests voor AccommodatieTypesController - 5 endpoints.
/// PUBLIC: GetAll, GetById
/// ADMIN: Create, Update, Delete
/// </summary>
public class AccommodatieTypesControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public AccommodatieTypesControllerTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    #region PUBLIC ENDPOINTS (2 tests)

    [Fact]
    public async Task GetAll_ShouldReturn200_WithTypesList()
    {
        // Arrange
        var types = new List<AccommodatieTypeDTO>
        {
            TestDataFactory.CreateTestAccommodatieType(1, "Geheel"),
            TestDataFactory.CreateTestAccommodatieType(2, "Slaapplek")
        };

        _factory.MockRepository
            .Setup(r => r.GetAllAccommodatieTypesAsync())
            .ReturnsAsync(types);

        // Act
        var response = await _client.GetAsync("/api/accommodatietypes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<AccommodatieTypeDTO>>();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetById_ShouldReturn200_WhenTypeExists()
    {
        // Arrange
        var type = TestDataFactory.CreateTestAccommodatieType(1, "Geheel");
        _factory.MockRepository
            .Setup(r => r.GetAccommodatieTypeByIdAsync(1))
            .ReturnsAsync(type);

        // Act
        var response = await _client.GetAsync("/api/accommodatietypes/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AccommodatieTypeDTO>();
        result.Should().NotBeNull();
        result!.TypeID.Should().Be(1);
        result.Naam.Should().Be("Geheel");
    }

    [Fact]
    public async Task GetById_ShouldReturn404_WhenTypeNotExists()
    {
        // Arrange
        _factory.MockRepository
            .Setup(r => r.GetAccommodatieTypeByIdAsync(999))
            .ReturnsAsync((AccommodatieTypeDTO?)null);

        // Act
        var response = await _client.GetAsync("/api/accommodatietypes/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region ADMIN ENDPOINTS (3 tests)

    [Fact]
    public async Task Create_ShouldReturn201_WhenSuccessful()
    {
        // Arrange
        var newType = TestDataFactory.CreateTestAccommodatieType(0, "Nieuw Type");
        _factory.MockRepository
            .Setup(r => r.CreateAccommodatieTypeAsync(It.IsAny<AccommodatieTypeDTO>()))
            .ReturnsAsync(1);

        _factory.MockRepository
            .Setup(r => r.CreateLogEntryAsync(It.IsAny<LogboekDTO>()))
            .ReturnsAsync(1);

        // Act
        var response = await _client.PostAsJsonAsync("/api/accommodatietypes", newType);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<AccommodatieTypeDTO>();
        result.Should().NotBeNull();
        result!.TypeID.Should().Be(1);
    }

    [Fact]
    public async Task Update_ShouldReturn204_WhenSuccessful()
    {
        // Arrange
        var type = TestDataFactory.CreateTestAccommodatieType(1, "Updated Type");
        _factory.MockRepository
            .Setup(r => r.GetAccommodatieTypeByIdAsync(1))
            .ReturnsAsync(TestDataFactory.CreateTestAccommodatieType(1));

        _factory.MockRepository
            .Setup(r => r.UpdateAccommodatieTypeAsync(It.IsAny<AccommodatieTypeDTO>()))
            .ReturnsAsync(true);

        _factory.MockRepository
            .Setup(r => r.CreateLogEntryAsync(It.IsAny<LogboekDTO>()))
            .ReturnsAsync(1);

        // Act
        var response = await _client.PutAsJsonAsync("/api/accommodatietypes/1", type);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Update_ShouldReturn400_WhenIdMismatch()
    {
        // Arrange
        var type = TestDataFactory.CreateTestAccommodatieType(2);

        // Act
        var response = await _client.PutAsJsonAsync("/api/accommodatietypes/1", type);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_ShouldReturn404_WhenTypeNotExists()
    {
        // Arrange
        var type = TestDataFactory.CreateTestAccommodatieType(999);
        _factory.MockRepository
            .Setup(r => r.GetAccommodatieTypeByIdAsync(999))
            .ReturnsAsync((AccommodatieTypeDTO?)null);

        // Act
        var response = await _client.PutAsJsonAsync("/api/accommodatietypes/999", type);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_ShouldReturn204_WhenNoUnitsWithType()
    {
        // Arrange
        var type = TestDataFactory.CreateTestAccommodatieType(1);
        _factory.MockRepository
            .Setup(r => r.GetAccommodatieTypeByIdAsync(1))
            .ReturnsAsync(type);

        _factory.MockRepository
            .Setup(r => r.GetAllGiteUnitsAsync())
            .ReturnsAsync(new List<VerhuurEenheidDTO>());

        _factory.MockRepository
            .Setup(r => r.DeleteAccommodatieTypeAsync(1))
            .ReturnsAsync(true);

        _factory.MockRepository
            .Setup(r => r.CreateLogEntryAsync(It.IsAny<LogboekDTO>()))
            .ReturnsAsync(1);

        // Act
        var response = await _client.DeleteAsync("/api/accommodatietypes/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_ShouldReturn400_WhenUnitsExistWithType()
    {
        // Arrange
        var type = TestDataFactory.CreateTestAccommodatieType(1);
        var unitWithType = TestDataFactory.CreateTestUnit(1);
        unitWithType.TypeID = 1;

        _factory.MockRepository
            .Setup(r => r.GetAccommodatieTypeByIdAsync(1))
            .ReturnsAsync(type);

        _factory.MockRepository
            .Setup(r => r.GetAllGiteUnitsAsync())
            .ReturnsAsync(new List<VerhuurEenheidDTO> { unitWithType });

        // Act
        var response = await _client.DeleteAsync("/api/accommodatietypes/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Delete_ShouldReturn404_WhenTypeNotExists()
    {
        // Arrange
        _factory.MockRepository
            .Setup(r => r.GetAccommodatieTypeByIdAsync(999))
            .ReturnsAsync((AccommodatieTypeDTO?)null);

        // Act
        var response = await _client.DeleteAsync("/api/accommodatietypes/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}
