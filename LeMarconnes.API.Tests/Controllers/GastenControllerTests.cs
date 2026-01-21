using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LeMarconnes.API.Tests.TestData;
using LeMarconnes.Shared.DTOs;
using Moq;

namespace LeMarconnes.API.Tests.Controllers;

/// <summary>
/// Integration tests voor GastenController - 6 endpoints.
/// PUBLIC/USER: GetById, Update
/// WEBHOOK: UpdateIBAN
/// ADMIN: GetAll, Create, Anonimiseer
/// </summary>
public class GastenControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public GastenControllerTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    #region PUBLIC/USER ENDPOINTS (2 tests)

    [Fact]
    public async Task GetById_ShouldReturn200_WhenGastExists()
    {
        // Arrange
        var gast = TestDataFactory.CreateTestGast(1);
        var gebruiker = TestDataFactory.CreateTestGebruiker(1, "User", 1); // GastID = 1

        _factory.MockRepository
            .Setup(r => r.GetGastByIdAsync(1))
            .ReturnsAsync(gast);

        _factory.MockRepository
            .Setup(r => r.GetGebruikerByEmailAsync("TestUser"))
            .ReturnsAsync(gebruiker);

        // Act
        var response = await _client.GetAsync("/api/gasten/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GastDTO>();
        result.Should().NotBeNull();
        result!.GastID.Should().Be(1);
        result.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetById_ShouldReturn404_WhenGastNotExists()
    {
        // Arrange
        _factory.MockRepository
            .Setup(r => r.GetGastByIdAsync(999))
            .ReturnsAsync((GastDTO?)null);

        // Act
        var response = await _client.GetAsync("/api/gasten/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_ShouldReturn204_WhenSuccessful()
    {
        // Arrange
        var gast = TestDataFactory.CreateTestGast(1);
        gast.Tel = "0687654321"; // Wijzig telefoonnummer
        var gebruiker = TestDataFactory.CreateTestGebruiker(1, "User", 1); // GastID = 1

        _factory.MockRepository
            .Setup(r => r.GetGastByIdAsync(1))
            .ReturnsAsync(TestDataFactory.CreateTestGast(1));

        _factory.MockRepository
            .Setup(r => r.GetGebruikerByEmailAsync("TestUser"))
            .ReturnsAsync(gebruiker);

        _factory.MockRepository
            .Setup(r => r.UpdateGastAsync(It.IsAny<GastDTO>()))
            .ReturnsAsync(true);

        _factory.MockRepository
            .Setup(r => r.CreateLogEntryAsync(It.IsAny<LogboekDTO>()))
            .ReturnsAsync(1);

        // Act
        var response = await _client.PutAsJsonAsync("/api/gasten/1", gast);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Update_ShouldReturn400_WhenIdMismatch()
    {
        // Arrange
        var gast = TestDataFactory.CreateTestGast(2);

        // Act
        var response = await _client.PutAsJsonAsync("/api/gasten/1", gast);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_ShouldReturn404_WhenGastNotExists()
    {
        // Arrange
        var gast = TestDataFactory.CreateTestGast(999);
        _factory.MockRepository
            .Setup(r => r.GetGastByIdAsync(999))
            .ReturnsAsync((GastDTO?)null);

        // Act
        var response = await _client.PutAsJsonAsync("/api/gasten/999", gast);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region WEBHOOK ENDPOINT (1 test)

    [Fact]
    public async Task UpdateIBAN_ShouldReturn200_WhenSuccessful()
    {
        // Arrange
        var request = TestDataFactory.CreateTestUpdateIBANRequest(1);
        var gast = TestDataFactory.CreateTestGast(1);

        _factory.MockRepository
            .Setup(r => r.GetGastByIdAsync(1))
            .ReturnsAsync(gast);

        _factory.MockRepository
            .Setup(r => r.UpdateGastIBANAsync(1, request.IBAN))
            .ReturnsAsync(true);

        _factory.MockRepository
            .Setup(r => r.CreateLogEntryAsync(It.IsAny<LogboekDTO>()))
            .ReturnsAsync(1);

        // Act
        var response = await _client.PostAsJsonAsync("/api/gasten/webhook/iban", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateIBAN_ShouldReturn404_WhenGastNotExists()
    {
        // Arrange
        var request = TestDataFactory.CreateTestUpdateIBANRequest(999);
        _factory.MockRepository
            .Setup(r => r.GetGastByIdAsync(999))
            .ReturnsAsync((GastDTO?)null);

        // Act
        var response = await _client.PostAsJsonAsync("/api/gasten/webhook/iban", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region ADMIN ENDPOINTS (3 tests)

    [Fact]
    public async Task GetAll_ShouldReturn200_WithList()
    {
        // Arrange
        var gasten = new List<GastDTO>
        {
            TestDataFactory.CreateTestGast(1),
            TestDataFactory.CreateTestGast(2, "test2@example.com")
        };

        _factory.MockRepository
            .Setup(r => r.GetAllGastenAsync())
            .ReturnsAsync(gasten);

        // Act
        var response = await _client.GetAsync("/api/gasten");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<GastDTO>>();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAll_WithZoek_ShouldReturn200_WhenGastFound()
    {
        // Arrange
        var gast = TestDataFactory.CreateTestGast(1);
        _factory.MockRepository
            .Setup(r => r.GetGastByEmailAsync("test@example.com"))
            .ReturnsAsync(gast);

        // Act
        var response = await _client.GetAsync("/api/gasten?zoek=test@example.com");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<GastDTO>>();
        result.Should().HaveCount(1);
        result![0].Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetAll_WithZoek_ShouldReturn404_WhenGastNotFound()
    {
        // Arrange
        _factory.MockRepository
            .Setup(r => r.GetGastByEmailAsync("notfound@example.com"))
            .ReturnsAsync((GastDTO?)null);

        // Act
        var response = await _client.GetAsync("/api/gasten?zoek=notfound@example.com");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_ShouldReturn201_WhenSuccessful()
    {
        // Arrange
        var newGast = TestDataFactory.CreateTestGast(0, "newgast@example.com");
        _factory.MockRepository
            .Setup(r => r.GetGastByEmailAsync(newGast.Email))
            .ReturnsAsync((GastDTO?)null);

        _factory.MockRepository
            .Setup(r => r.CreateGastAsync(It.IsAny<GastDTO>()))
            .ReturnsAsync(1);

        _factory.MockRepository
            .Setup(r => r.CreateLogEntryAsync(It.IsAny<LogboekDTO>()))
            .ReturnsAsync(1);

        // Act
        var response = await _client.PostAsJsonAsync("/api/gasten", newGast);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<GastDTO>();
        result.Should().NotBeNull();
        result!.GastID.Should().Be(1);
    }

    [Fact]
    public async Task Create_ShouldReturn409_WhenEmailAlreadyExists()
    {
        // Arrange
        var newGast = TestDataFactory.CreateTestGast(0, "test@example.com");
        var bestaandeGast = TestDataFactory.CreateTestGast(1, "test@example.com");

        _factory.MockRepository
            .Setup(r => r.GetGastByEmailAsync("test@example.com"))
            .ReturnsAsync(bestaandeGast);

        // Act
        var response = await _client.PostAsJsonAsync("/api/gasten", newGast);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Anonimiseer_ShouldReturn204_WhenSuccessful()
    {
        // Arrange
        var gast = TestDataFactory.CreateTestGast(1);
        _factory.MockRepository
            .Setup(r => r.GetGastByIdAsync(1))
            .ReturnsAsync(gast);

        _factory.MockRepository
            .Setup(r => r.AnonimiseerGastAsync(1))
            .ReturnsAsync(true);

        _factory.MockRepository
            .Setup(r => r.CreateLogEntryAsync(It.IsAny<LogboekDTO>()))
            .ReturnsAsync(1);

        // Act
        var response = await _client.DeleteAsync("/api/gasten/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Anonimiseer_ShouldReturn404_WhenGastNotExists()
    {
        // Arrange
        _factory.MockRepository
            .Setup(r => r.GetGastByIdAsync(999))
            .ReturnsAsync((GastDTO?)null);

        // Act
        var response = await _client.DeleteAsync("/api/gasten/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}
