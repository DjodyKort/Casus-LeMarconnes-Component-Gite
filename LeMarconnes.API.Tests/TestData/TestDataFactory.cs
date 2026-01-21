using LeMarconnes.Shared.DTOs;
using LeMarconnes.API.Controllers;

namespace LeMarconnes.API.Tests.TestData;

/// <summary>
/// Factory voor het maken van test data.
/// </summary>
public static class TestDataFactory
{
    public static VerhuurEenheidDTO CreateTestUnit(int id = 1, string naam = "Test Gîte", int? parentId = null)
    {
        return new VerhuurEenheidDTO
        {
            EenheidID = id,
            Naam = naam,
            TypeID = parentId.HasValue ? 2 : 1, // Type 2 = Slaapplek, Type 1 = Geheel
            MaxCapaciteit = 6,
            Type = new AccommodatieTypeDTO(parentId.HasValue ? 2 : 1, parentId.HasValue ? "Slaapplek" : "Geheel"),
            ParentEenheidID = parentId
        };
    }

    public static GastDTO CreateTestGast(int id = 1, string email = "test@example.com")
    {
        return new GastDTO
        {
            GastID = id,
            Naam = "Test Gast",
            Email = email,
            Tel = "0612345678",
            Straat = "Teststraat",
            Huisnr = "1",
            Postcode = "1234AB",
            Plaats = "Teststad",
            Land = "Nederland",
            IBAN = id > 0 ? "NL00TEST1234567890" : null
        };
    }

    public static ReserveringDTO CreateTestReservering(int id = 1, int gastId = 1, int eenheidId = 1)
    {
        return new ReserveringDTO
        {
            ReserveringID = id,
            GastID = gastId,
            EenheidID = eenheidId,
            PlatformID = 1,
            Startdatum = DateTime.Today.AddDays(7),
            Einddatum = DateTime.Today.AddDays(14),
            Status = "Gereserveerd",
            Gast = CreateTestGast(gastId),
            Eenheid = CreateTestUnit(eenheidId),
            Platform = CreateTestPlatform(1)
        };
    }

    public static ReserveringDetailDTO CreateTestReserveringDetail(int id = 1, int reserveringId = 1, int categorieId = 1)
    {
        return new ReserveringDetailDTO
        {
            DetailID = id,
            ReserveringID = reserveringId,
            CategorieID = categorieId,
            Aantal = 7,
            PrijsOpMoment = 700.00m,
            Categorie = CreateTestTariefCategorie(categorieId)
        };
    }

    public static PlatformDTO CreateTestPlatform(int id = 1)
    {
        var namen = new[] { "Eigen Site", "Booking.com", "Airbnb" };
        return new PlatformDTO
        {
            PlatformID = id,
            Naam = id <= namen.Length ? namen[id - 1] : "Test Platform",
            CommissiePercentage = id == 1 ? 0 : 15
        };
    }

    public static TariefDTO CreateTestTarief(int id = 1, int typeId = 1, int categorieId = 1, int platformId = 1)
    {
        return new TariefDTO
        {
            TariefID = id,
            TypeID = typeId,
            CategorieID = categorieId,
            PlatformID = platformId,
            Prijs = 100.00m,
            TaxStatus = false,
            TaxTarief = 1.50m,
            GeldigVan = DateTime.Today.AddMonths(-1),
            GeldigTot = DateTime.Today.AddMonths(12),
            Type = new AccommodatieTypeDTO(typeId, typeId == 1 ? "Geheel" : "Slaapplek"),
            Categorie = new TariefCategorieDTO(categorieId, categorieId == 1 ? "Logies" : "Toeristenbelasting"),
            Platform = CreateTestPlatform(platformId)
        };
    }

    public static AccommodatieTypeDTO CreateTestAccommodatieType(int id = 1, string? naam = null)
    {
        return new AccommodatieTypeDTO(id, naam ?? (id == 1 ? "Geheel" : "Slaapplek"));
    }

    public static TariefCategorieDTO CreateTestTariefCategorie(int id = 1, string? naam = null)
    {
        return new TariefCategorieDTO(id, naam ?? (id == 1 ? "Logies" : "Toeristenbelasting"));
    }

    public static GebruikerDTO CreateTestGebruiker(int id = 1, string rol = "User", int? gastId = null)
    {
        // Hash the password "password123" using SHA256 (same as controller)
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes("password123"));
        var wachtwoordHash = Convert.ToBase64String(hashBytes);

        return new GebruikerDTO
        {
            GebruikerID = id,
            GastID = gastId,
            Email = $"gebruiker{id}@example.com",
            Rol = rol,
            WachtwoordHash = wachtwoordHash,
            Gast = gastId.HasValue ? CreateTestGast(gastId.Value) : null
        };
    }

    public static LogboekDTO CreateTestLogboek(int id = 1, string actie = "TEST_ACTIE", string tabel = "TEST_TABEL", int recordId = 1)
    {
        return new LogboekDTO
        {
            LogID = id,
            GebruikerID = 1,
            Tijdstip = DateTime.Now,
            Actie = actie,
            TabelNaam = tabel,
            RecordID = recordId,
            OudeWaarde = "Oud",
            NieuweWaarde = "Nieuw"
        };
    }

    public static BoekingRequestDTO CreateTestBoekingRequest(int eenheidId = 1, int platformId = 1)
    {
        return new BoekingRequestDTO
        {
            // Gastgegevens (flattened)
            GastNaam = "Test Gast",
            GastEmail = "test@example.com",
            GastTel = "0612345678",
            GastStraat = "Teststraat",
            GastHuisnr = "1",
            GastPostcode = "1234AB",
            GastPlaats = "Teststad",
            GastLand = "Nederland",
            // Boekinggegevens
            EenheidID = eenheidId,
            PlatformID = platformId,
            StartDatum = DateTime.Today.AddDays(7),
            EindDatum = DateTime.Today.AddDays(14),
            AantalPersonen = 4
        };
    }

    public static BoekingResponseDTO CreateTestBoekingResponse(int reserveringId = 1)
    {
        return BoekingResponseDTO.Success(
            reserveringId,
            "Test Gîte",
            DateTime.Today.AddDays(7),
            DateTime.Today.AddDays(14),
            700.00m
        );
    }

    public static UpdateIBANRequestDTO CreateTestUpdateIBANRequest(int gastId = 1, string iban = "NL91ABNA0417164300")
    {
        return new UpdateIBANRequestDTO
        {
            GastID = gastId,
            IBAN = iban
        };
    }

    public static RegisterGebruikerRequestDTO CreateTestRegisterRequest(string email = "newuser@example.com", string password = "SecurePass123!")
    {
        return new RegisterGebruikerRequestDTO
        {
            Email = email,
            Wachtwoord = password
        };
    }
}
