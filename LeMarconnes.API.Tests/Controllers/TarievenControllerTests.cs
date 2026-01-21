using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LeMarconnes.API.Controllers;
using LeMarconnes.API.Tests.TestData;
using LeMarconnes.Shared.DTOs;
using Moq;

namespace LeMarconnes.API.Tests.Controllers;

/// <summary>
/// Integration tests voor TarievenController - 7 endpoints.
/// PUBLIC: GetAll, GetGeldigTarief, BerekenPrijs, GetById
/// ADMIN: Create, Update, Delete
/// </summary>
public class TarievenControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public TarievenControllerTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    #region PUBLIC ENDPOINTS (4 tests)

    [Fact]
    public async Task GetAll_ShouldReturn200_WithTarievenList()
    {
        // Arrange
        var tarieven = new List<TariefDTO>
        {
            TestDataFactory.CreateTestTarief(1, 1, 1, 1),
            TestDataFactory.CreateTestTarief(2, 1, 2, 1),
            TestDataFactory.CreateTestTarief(3, 2, 1, 1)
        };

        _factory.MockRepository
            .Setup(r => r.GetAllTarievenAsync())
            .ReturnsAsync(tarieven);

        // Act
        var response = await _client.GetAsync("/api/tarieven");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<TariefDTO>>();
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetGeldigTarief_ShouldReturn200_WhenTariefExists()
    {
        // Arrange
        var tarief = TestDataFactory.CreateTestTarief(1, 1, 1, 1);

        _factory.MockRepository
            .Setup(r => r.GetTariefAsync(1, 1, It.IsAny<DateTime>()))
            .ReturnsAsync(tarief);

        // Act
        var response = await _client.GetAsync($"/api/tarieven/1/1?datum={DateTime.Today:yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<TariefDTO>();
        result.Should().NotBeNull();
        result!.TypeID.Should().Be(1);
        result.PlatformID.Should().Be(1);
    }

    [Fact]
    public async Task GetGeldigTarief_ShouldReturn404_WhenTariefNotExists()
    {
        // Arrange
        _factory.MockRepository
            .Setup(r => r.GetTariefAsync(999, 999, It.IsAny<DateTime>()))
            .ReturnsAsync((TariefDTO?)null);

        // Act
        var response = await _client.GetAsync("/api/tarieven/999/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task BerekenPrijs_ShouldReturn200_WithPrijsberekening()
    {
        // Arrange
        var unit = TestDataFactory.CreateTestUnit(1);
        var tarief = TestDataFactory.CreateTestTarief(1, 1, 1, 1);

        _factory.MockRepository
            .Setup(r => r.GetUnitByIdAsync(1))
            .ReturnsAsync(unit);

        _factory.MockRepository
            .Setup(r => r.GetTariefAsync(1, 1, It.IsAny<DateTime>()))
            .ReturnsAsync(tarief);

        // Act
        var startDatum = DateTime.Today.AddDays(7);
        var eindDatum = DateTime.Today.AddDays(14);
        var response = await _client.GetAsync(
            $"/api/tarieven/berekenen?eenheidId=1&platformId=1&startDatum={startDatum:yyyy-MM-dd}&eindDatum={eindDatum:yyyy-MM-dd}&aantalPersonen=4");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PrijsberekeningResponseDTO>();
        result.Should().NotBeNull();
        result!.AantalNachten.Should().Be(7);
        result.AantalPersonen.Should().Be(4);
        result.TotaalPrijs.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetById_ShouldReturn200_WhenTariefExists()
    {
        // Arrange
        var tarief = TestDataFactory.CreateTestTarief(1);
        _factory.MockRepository
            .Setup(r => r.GetTariefByIdAsync(1))
            .ReturnsAsync(tarief);

        // Act
        var response = await _client.GetAsync("/api/tarieven/details/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<TariefDTO>();
        result.Should().NotBeNull();
        result!.TariefID.Should().Be(1);
    }

    [Fact]
    public async Task GetById_ShouldReturn404_WhenTariefNotExists()
    {
        // Arrange
        _factory.MockRepository
            .Setup(r => r.GetTariefByIdAsync(999))
            .ReturnsAsync((TariefDTO?)null);

        // Act
        var response = await _client.GetAsync("/api/tarieven/details/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region ADMIN ENDPOINTS (3 tests)

    [Fact]
    public async Task Create_ShouldReturn201_WhenSuccessful()
    {
        // Arrange
        var newTarief = TestDataFactory.CreateTestTarief(0);
        _factory.MockRepository
            .Setup(r => r.CreateTariefAsync(It.IsAny<TariefDTO>()))
            .ReturnsAsync(1);

        _factory.MockRepository
            .Setup(r => r.CreateLogEntryAsync(It.IsAny<LogboekDTO>()))
            .ReturnsAsync(1);

        // Act
        var response = await _client.PostAsJsonAsync("/api/tarieven", newTarief);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<TariefDTO>();
        result.Should().NotBeNull();
        result!.TariefID.Should().Be(1);
    }

    [Fact]
    public async Task Update_ShouldReturn204_WhenSuccessful()
    {
        // Arrange
        var tarief = TestDataFactory.CreateTestTarief(1);
        tarief.Prijs = 150.00m; // Update prijs

        _factory.MockRepository
            .Setup(r => r.GetTariefByIdAsync(1))
            .ReturnsAsync(TestDataFactory.CreateTestTarief(1));

        _factory.MockRepository
            .Setup(r => r.UpdateTariefAsync(It.IsAny<TariefDTO>()))
            .ReturnsAsync(true);

        _factory.MockRepository
            .Setup(r => r.CreateLogEntryAsync(It.IsAny<LogboekDTO>()))
            .ReturnsAsync(1);

        // Act
        var response = await _client.PutAsJsonAsync("/api/tarieven/1", tarief);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Update_ShouldReturn400_WhenIdMismatch()
    {
        // Arrange
        var tarief = TestDataFactory.CreateTestTarief(2);

        // Act
        var response = await _client.PutAsJsonAsync("/api/tarieven/1", tarief);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_ShouldReturn404_WhenTariefNotExists()
    {
        // Arrange
        var tarief = TestDataFactory.CreateTestTarief(999);
        _factory.MockRepository
            .Setup(r => r.GetTariefByIdAsync(999))
            .ReturnsAsync((TariefDTO?)null);

        // Act
        var response = await _client.PutAsJsonAsync("/api/tarieven/999", tarief);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_ShouldReturn204_WhenSuccessful()
    {
        // Arrange
        var tarief = TestDataFactory.CreateTestTarief(1);
        _factory.MockRepository
            .Setup(r => r.GetTariefByIdAsync(1))
            .ReturnsAsync(tarief);

        _factory.MockRepository
            .Setup(r => r.DeleteTariefAsync(1))
            .ReturnsAsync(true);

        _factory.MockRepository
            .Setup(r => r.CreateLogEntryAsync(It.IsAny<LogboekDTO>()))
            .ReturnsAsync(1);

        // Act
        var response = await _client.DeleteAsync("/api/tarieven/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_ShouldReturn404_WhenTariefNotExists()
    {
        // Arrange
        _factory.MockRepository
            .Setup(r => r.GetTariefByIdAsync(999))
            .ReturnsAsync((TariefDTO?)null);

        // Act
        var response = await _client.DeleteAsync("/api/tarieven/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}
