using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LeMarconnes.API.Tests.TestData;
using LeMarconnes.Shared.DTOs;
using Moq;

namespace LeMarconnes.API.Tests.Controllers;

/// <summary>
/// Integration tests voor VerhuurEenhedenController - 5 endpoints.
/// PUBLIC: GetAll, GetById
/// ADMIN: Create, Update, Delete
/// </summary>
public class VerhuurEenhedenControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public VerhuurEenhedenControllerTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    #region PUBLIC ENDPOINTS (2 tests)

    [Fact]
    public async Task GetAll_ShouldReturn200_WithUnitsList()
    {
        // Arrange
        var units = new List<VerhuurEenheidDTO>
        {
            TestDataFactory.CreateTestUnit(1, "Gîte A"),
            TestDataFactory.CreateTestUnit(2, "Gîte B"),
            TestDataFactory.CreateTestUnit(3, "Slaapplek 1", 1)
        };

        _factory.MockRepository
            .Setup(r => r.GetAllGiteUnitsAsync())
            .ReturnsAsync(units);

        // Act
        var response = await _client.GetAsync("/api/verhuureenheden");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<VerhuurEenheidDTO>>();
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetById_ShouldReturn200_WhenUnitExists()
    {
        // Arrange
        var unit = TestDataFactory.CreateTestUnit(1, "Test Gîte");
        _factory.MockRepository
            .Setup(r => r.GetUnitByIdAsync(1))
            .ReturnsAsync(unit);

        // Act
        var response = await _client.GetAsync("/api/verhuureenheden/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<VerhuurEenheidDTO>();
        result.Should().NotBeNull();
        result!.EenheidID.Should().Be(1);
        result.Naam.Should().Be("Test Gîte");
    }

    [Fact]
    public async Task GetById_ShouldReturn404_WhenUnitNotExists()
    {
        // Arrange
        _factory.MockRepository
            .Setup(r => r.GetUnitByIdAsync(999))
            .ReturnsAsync((VerhuurEenheidDTO?)null);

        // Act
        var response = await _client.GetAsync("/api/verhuureenheden/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region ADMIN ENDPOINTS (3 tests)

    [Fact]
    public async Task Create_ShouldReturn201_WhenSuccessful()
    {
        // Arrange
        var newUnit = TestDataFactory.CreateTestUnit(0, "Nieuw Gîte");
        _factory.MockRepository
            .Setup(r => r.CreateUnitAsync(It.IsAny<VerhuurEenheidDTO>()))
            .ReturnsAsync(1);

        _factory.MockRepository
            .Setup(r => r.CreateLogEntryAsync(It.IsAny<LogboekDTO>()))
            .ReturnsAsync(1);

        // Act
        var response = await _client.PostAsJsonAsync("/api/verhuureenheden", newUnit);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<VerhuurEenheidDTO>();
        result.Should().NotBeNull();
        result!.EenheidID.Should().Be(1);
    }

    [Fact]
    public async Task Update_ShouldReturn204_WhenSuccessful()
    {
        // Arrange
        var unit = TestDataFactory.CreateTestUnit(1, "Updated Gîte");
        _factory.MockRepository
            .Setup(r => r.GetUnitByIdAsync(1))
            .ReturnsAsync(TestDataFactory.CreateTestUnit(1));

        _factory.MockRepository
            .Setup(r => r.UpdateUnitAsync(It.IsAny<VerhuurEenheidDTO>()))
            .ReturnsAsync(true);

        _factory.MockRepository
            .Setup(r => r.CreateLogEntryAsync(It.IsAny<LogboekDTO>()))
            .ReturnsAsync(1);

        // Act
        var response = await _client.PutAsJsonAsync("/api/verhuureenheden/1", unit);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Update_ShouldReturn400_WhenIdMismatch()
    {
        // Arrange
        var unit = TestDataFactory.CreateTestUnit(2);

        // Act
        var response = await _client.PutAsJsonAsync("/api/verhuureenheden/1", unit);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_ShouldReturn404_WhenUnitNotExists()
    {
        // Arrange
        var unit = TestDataFactory.CreateTestUnit(999);
        _factory.MockRepository
            .Setup(r => r.GetUnitByIdAsync(999))
            .ReturnsAsync((VerhuurEenheidDTO?)null);

        // Act
        var response = await _client.PutAsJsonAsync("/api/verhuureenheden/999", unit);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_ShouldReturn204_WhenNoActiveReservations()
    {
        // Arrange
        var unit = TestDataFactory.CreateTestUnit(1);
        _factory.MockRepository
            .Setup(r => r.GetUnitByIdAsync(1))
            .ReturnsAsync(unit);

        _factory.MockRepository
            .Setup(r => r.GetReservationsForUnitAsync(1, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<ReserveringDTO>());

        _factory.MockRepository
            .Setup(r => r.DeleteUnitAsync(1))
            .ReturnsAsync(true);

        _factory.MockRepository
            .Setup(r => r.CreateLogEntryAsync(It.IsAny<LogboekDTO>()))
            .ReturnsAsync(1);

        // Act
        var response = await _client.DeleteAsync("/api/verhuureenheden/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_ShouldReturn400_WhenActiveReservationsExist()
    {
        // Arrange
        var unit = TestDataFactory.CreateTestUnit(1);
        var actieveReservering = TestDataFactory.CreateTestReservering(1, 1, 1);
        actieveReservering.Status = "Gereserveerd";

        _factory.MockRepository
            .Setup(r => r.GetUnitByIdAsync(1))
            .ReturnsAsync(unit);

        _factory.MockRepository
            .Setup(r => r.GetReservationsForUnitAsync(1, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<ReserveringDTO> { actieveReservering });

        // Act
        var response = await _client.DeleteAsync("/api/verhuureenheden/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Delete_ShouldReturn404_WhenUnitNotExists()
    {
        // Arrange
        _factory.MockRepository
            .Setup(r => r.GetUnitByIdAsync(999))
            .ReturnsAsync((VerhuurEenheidDTO?)null);

        // Act
        var response = await _client.DeleteAsync("/api/verhuureenheden/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}
