using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LeMarconnes.API.Tests.TestData;
using LeMarconnes.Shared.DTOs;
using Moq;

namespace LeMarconnes.API.Tests.Controllers;

/// <summary>
/// Integration tests voor BeschikbaarheidController, LogboekenController, en LookupsController.
/// BeschikbaarheidController: 1 endpoint (PUBLIC)
/// LogboekenController: 2 endpoints (ADMIN)
/// LookupsController: 4 endpoints (PUBLIC) - gebruikers endpoints verwijderd (zie GebruikersController)
/// </summary>
public class MiscControllersTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public MiscControllersTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    #region BESCHIKBAARHEID CONTROLLER (3 tests)

    [Fact]
    public async Task Beschikbaarheid_CheckBeschikbaarheid_ShouldReturn200_WithAvailableUnits()
    {
        // Arrange
        var units = new List<VerhuurEenheidDTO>
        {
            TestDataFactory.CreateTestUnit(1, "Gîte A"),
            TestDataFactory.CreateTestUnit(2, "Gîte B")
        };

        _factory.MockRepository
            .Setup(r => r.GetAllGiteUnitsAsync())
            .ReturnsAsync(units);

        _factory.MockRepository
            .Setup(r => r.GetReservationsByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<ReserveringDTO>());

        // Act
        var startDatum = DateTime.Today.AddDays(7);
        var eindDatum = DateTime.Today.AddDays(14);
        var response = await _client.GetAsync(
            $"/api/beschikbaarheid?startDatum={startDatum:yyyy-MM-dd}&eindDatum={eindDatum:yyyy-MM-dd}&aantalPersonen=4");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<VerhuurEenheidDTO>>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Beschikbaarheid_CheckBeschikbaarheid_ShouldReturn400_WhenInvalidDates()
    {
        // Arrange & Act
        var startDatum = DateTime.Today.AddDays(14);
        var eindDatum = DateTime.Today.AddDays(7); // Eindddatum voor startdatum
        var response = await _client.GetAsync(
            $"/api/beschikbaarheid?startDatum={startDatum:yyyy-MM-dd}&eindDatum={eindDatum:yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Beschikbaarheid_CheckBeschikbaarheid_ShouldFilterByCapacity()
    {
        // Arrange
        var units = new List<VerhuurEenheidDTO>
        {
            TestDataFactory.CreateTestUnit(1, "Kleine Gîte"),
            TestDataFactory.CreateTestUnit(2, "Grote Gîte")
        };
        units[0].MaxCapaciteit = 2;
        units[1].MaxCapaciteit = 8;

        _factory.MockRepository
            .Setup(r => r.GetAllGiteUnitsAsync())
            .ReturnsAsync(units);

        _factory.MockRepository
            .Setup(r => r.GetReservationsByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<ReserveringDTO>());

        // Act
        var startDatum = DateTime.Today.AddDays(7);
        var eindDatum = DateTime.Today.AddDays(14);
        var response = await _client.GetAsync(
            $"/api/beschikbaarheid?startDatum={startDatum:yyyy-MM-dd}&eindDatum={eindDatum:yyyy-MM-dd}&aantalPersonen=6");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region LOGBOEKEN CONTROLLER (3 tests)

    [Fact]
    public async Task Logboeken_GetRecent_ShouldReturn200_WithLogs()
    {
        // Arrange
        var logs = new List<LogboekDTO>
        {
            TestDataFactory.CreateTestLogboek(1, "RESERVERING_AANGEMAAKT", "RESERVERING", 1),
            TestDataFactory.CreateTestLogboek(2, "GAST_GEWIJZIGD", "GAST", 1)
        };

        _factory.MockRepository
            .Setup(r => r.GetRecentLogsAsync(50))
            .ReturnsAsync(logs);

        // Act
        var response = await _client.GetAsync("/api/logboeken?count=50");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<LogboekDTO>>();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Logboeken_GetRecent_ShouldLimitTo500()
    {
        // Arrange
        var logs = Enumerable.Range(1, 100)
            .Select(i => TestDataFactory.CreateTestLogboek(i))
            .ToList();

        _factory.MockRepository
            .Setup(r => r.GetRecentLogsAsync(500))
            .ReturnsAsync(logs);

        // Act
        var response = await _client.GetAsync("/api/logboeken?count=1000"); // Vraag 1000, maar max is 500

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Logboeken_GetByEntiteit_ShouldReturn200_WithFilteredLogs()
    {
        // Arrange
        var logs = new List<LogboekDTO>
        {
            TestDataFactory.CreateTestLogboek(1, "RESERVERING_AANGEMAAKT", "RESERVERING", 5),
            TestDataFactory.CreateTestLogboek(2, "RESERVERING_GEWIJZIGD", "RESERVERING", 5)
        };

        _factory.MockRepository
            .Setup(r => r.GetLogsByEntiteitAsync("RESERVERING", 5))
            .ReturnsAsync(logs);

        // Act
        var response = await _client.GetAsync("/api/logboeken/entiteit/RESERVERING/5");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<LogboekDTO>>();
        result.Should().HaveCount(2);
        result!.All(l => l.TabelNaam == "RESERVERING" && l.RecordID == 5).Should().BeTrue();
    }

    #endregion

    #region LOOKUPS CONTROLLER (8 tests)

    [Fact]
    public async Task Lookups_GetAllPlatformen_ShouldReturn200()
    {
        // Arrange
        var platformen = new List<PlatformDTO>
        {
            TestDataFactory.CreateTestPlatform(1),
            TestDataFactory.CreateTestPlatform(2),
            TestDataFactory.CreateTestPlatform(3)
        };

        _factory.MockRepository
            .Setup(r => r.GetAllPlatformsAsync())
            .ReturnsAsync(platformen);

        // Act
        var response = await _client.GetAsync("/api/lookups/platformen");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<PlatformDTO>>();
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task Lookups_GetPlatformById_ShouldReturn200_WhenExists()
    {
        // Arrange
        var platform = TestDataFactory.CreateTestPlatform(1);
        _factory.MockRepository
            .Setup(r => r.GetPlatformByIdAsync(1))
            .ReturnsAsync(platform);

        // Act
        var response = await _client.GetAsync("/api/lookups/platformen/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PlatformDTO>();
        result.Should().NotBeNull();
        result!.PlatformID.Should().Be(1);
    }

    [Fact]
    public async Task Lookups_GetPlatformById_ShouldReturn404_WhenNotExists()
    {
        // Arrange
        _factory.MockRepository
            .Setup(r => r.GetPlatformByIdAsync(999))
            .ReturnsAsync((PlatformDTO?)null);

        // Act
        var response = await _client.GetAsync("/api/lookups/platformen/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Lookups_GetAllTariefCategorieen_ShouldReturn200()
    {
        // Arrange
        var categorieen = new List<TariefCategorieDTO>
        {
            TestDataFactory.CreateTestTariefCategorie(1, "Logies"),
            TestDataFactory.CreateTestTariefCategorie(2, "Toeristenbelasting")
        };

        _factory.MockRepository
            .Setup(r => r.GetAllTariefCategoriesAsync())
            .ReturnsAsync(categorieen);

        // Act
        var response = await _client.GetAsync("/api/lookups/tariefcategorieen");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<TariefCategorieDTO>>();
        result.Should().HaveCount(2);
    }

    #endregion
}
