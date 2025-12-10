// ======== Imports ========
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySqlConnector;
using Microsoft.Extensions.Configuration;
using LeMarconnes.Shared.DTOs;
using LeMarconnes.API.DAL.Interfaces;

// ======== Namespace ========
namespace LeMarconnes.API.DAL.Repositories
{
    // Repository voor Gîte database operaties. Gebruikt ADO.NET met MySqlConnector.
    // Alle queries gebruiken parameterized statements (SQL injection preventie).
    public class GiteRepository : IGiteRepository
    {
        // ==== Properties ====
        private readonly string _connectionString;

        // ==== Constructor ====
        public GiteRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found in appsettings.json");
        }

        // ============================================================
        // ==== VERHUUR EENHEDEN METHODS ====
        // ============================================================

        // Haal alle Gîte eenheden (TypeID 1=Geheel, 2=Slaapplek)
        public async Task<List<VerhuurEenheidDTO>> GetAllGiteUnitsAsync()
        {
            // ==== Declaring Variables ====
            var units = new List<VerhuurEenheidDTO>();
            const string sql = @"
                SELECT EenheidID, Naam, TypeID, MaxCapaciteit, ParentEenheidID 
                FROM VERHUUR_EENHEID 
                WHERE TypeID IN (1, 2)
                ORDER BY EenheidID";

            // ==== Start of Function ====
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                units.Add(MapToVerhuurEenheidDTO(reader));
            }

            return units;
        }

        // Haal specifieke eenheid op via ID
        public async Task<VerhuurEenheidDTO?> GetUnitByIdAsync(int eenheidId)
        {
            // ==== Declaring Variables ====
            VerhuurEenheidDTO? unit = null;
            const string sql = @"
                SELECT EenheidID, Naam, TypeID, MaxCapaciteit, ParentEenheidID 
                FROM VERHUUR_EENHEID 
                WHERE EenheidID = @EenheidID";

            // ==== Start of Function ====
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@EenheidID", eenheidId);

            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                unit = MapToVerhuurEenheidDTO(reader);
            }

            return unit;
        }

        // Haal children voor een parent (slaapplekken voor de Gîte)
        public async Task<List<VerhuurEenheidDTO>> GetChildUnitsAsync(int parentId)
        {
            // ==== Declaring Variables ====
            var units = new List<VerhuurEenheidDTO>();
            const string sql = @"
                SELECT EenheidID, Naam, TypeID, MaxCapaciteit, ParentEenheidID 
                FROM VERHUUR_EENHEID 
                WHERE ParentEenheidID = @ParentID
                ORDER BY EenheidID";

            // ==== Start of Function ====
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@ParentID", parentId);

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                units.Add(MapToVerhuurEenheidDTO(reader));
            }

            return units;
        }

        // ============================================================
        // ==== RESERVERINGEN METHODS ====
        // ============================================================

        // Haal alle reserveringen
        public async Task<List<ReserveringDTO>> GetAllReserveringenAsync()
        {
            // ==== Declaring Variables ====
            var reserveringen = new List<ReserveringDTO>();
            const string sql = @"
                SELECT ReserveringID, GastID, EenheidID, PlatformID, Startdatum, Einddatum, Status
                FROM RESERVERING
                ORDER BY Startdatum DESC";

            // ==== Start of Function ====
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                reserveringen.Add(MapToReserveringDTO(reader));
            }

            return reserveringen;
        }

        // Haal reservering op via ID
        public async Task<ReserveringDTO?> GetReserveringByIdAsync(int reserveringId)
        {
            // ==== Declaring Variables ====
            ReserveringDTO? reservering = null;
            const string sql = @"
                SELECT ReserveringID, GastID, EenheidID, PlatformID, Startdatum, Einddatum, Status
                FROM RESERVERING
                WHERE ReserveringID = @ReserveringID";

            // ==== Start of Function ====
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@ReserveringID", reserveringId);

            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                reservering = MapToReserveringDTO(reader);
            }

            return reservering;
        }

        // Haal overlappende reserveringen voor een periode (voor beschikbaarheidscheck)
        // Overlap: (Start LT EindParam) AND (Eind GT StartParam)
        public async Task<List<ReserveringDTO>> GetReservationsByDateRangeAsync(DateTime startDatum, DateTime eindDatum)
        {
            // ==== Declaring Variables ====
            var reserveringen = new List<ReserveringDTO>();
            const string sql = @"
                SELECT ReserveringID, GastID, EenheidID, PlatformID, Startdatum, Einddatum, Status
                FROM RESERVERING
                WHERE Startdatum < @EindDatum 
                  AND Einddatum > @StartDatum
                  AND Status != 'Geannuleerd'";

            // ==== Start of Function ====
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@StartDatum", startDatum);
            command.Parameters.AddWithValue("@EindDatum", eindDatum);

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                reserveringen.Add(MapToReserveringDTO(reader));
            }

            return reserveringen;
        }

        // Haal reserveringen voor een eenheid in een periode
        public async Task<List<ReserveringDTO>> GetReservationsForUnitAsync(int eenheidId, DateTime startDatum, DateTime eindDatum)
        {
            // ==== Declaring Variables ====
            var reserveringen = new List<ReserveringDTO>();
            const string sql = @"
                SELECT ReserveringID, GastID, EenheidID, PlatformID, Startdatum, Einddatum, Status
                FROM RESERVERING
                WHERE EenheidID = @EenheidID
                  AND Startdatum < @EindDatum 
                  AND Einddatum > @StartDatum
                  AND Status != 'Geannuleerd'";

            // ==== Start of Function ====
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@EenheidID", eenheidId);
            command.Parameters.AddWithValue("@StartDatum", startDatum);
            command.Parameters.AddWithValue("@EindDatum", eindDatum);

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                reserveringen.Add(MapToReserveringDTO(reader));
            }

            return reserveringen;
        }

        // Haal reserveringen voor een gast
        public async Task<List<ReserveringDTO>> GetReservationsForGastAsync(int gastId)
        {
            // ==== Declaring Variables ====
            var reserveringen = new List<ReserveringDTO>();
            const string sql = @"
                SELECT ReserveringID, GastID, EenheidID, PlatformID, Startdatum, Einddatum, Status
                FROM RESERVERING
                WHERE GastID = @GastID
                ORDER BY Startdatum DESC";

            // ==== Start of Function ====
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@GastID", gastId);

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                reserveringen.Add(MapToReserveringDTO(reader));
            }

            return reserveringen;
        }

        // Maak reservering, retourneer nieuw ID
        public async Task<int> CreateReservationAsync(ReserveringDTO reservering)
        {
            // ==== Declaring Variables ====
            const string sql = @"
                INSERT INTO RESERVERING (GastID, EenheidID, PlatformID, Startdatum, Einddatum, Status)
                VALUES (@GastID, @EenheidID, @PlatformID, @Startdatum, @Einddatum, @Status);
                SELECT LAST_INSERT_ID();";

            // ==== Start of Function ====
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@GastID", reservering.GastID);
            command.Parameters.AddWithValue("@EenheidID", reservering.EenheidID);
            command.Parameters.AddWithValue("@PlatformID", reservering.PlatformID);
            command.Parameters.AddWithValue("@Startdatum", reservering.Startdatum);
            command.Parameters.AddWithValue("@Einddatum", reservering.Einddatum);
            command.Parameters.AddWithValue("@Status", reservering.Status);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        // Update reserveringsstatus
        public async Task<bool> UpdateReservationStatusAsync(int reserveringId, string status)
        {
            // ==== Declaring Variables ====
            const string sql = @"
                UPDATE RESERVERING 
                SET Status = @Status 
                WHERE ReserveringID = @ReserveringID";

            // ==== Start of Function ====
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@ReserveringID", reserveringId);
            command.Parameters.AddWithValue("@Status", status);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        // Verwijder reservering
        public async Task<bool> DeleteReservationAsync(int reserveringId)
        {
            // ==== Declaring Variables ====
            const string sql = @"DELETE FROM RESERVERING WHERE ReserveringID = @ReserveringID";

            // ==== Start of Function ====
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@ReserveringID", reserveringId);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        // Maak detailregel (kostenpost), retourneer nieuw ID
        public async Task<int> CreateReservationDetailAsync(ReserveringDetailDTO detail)
        {
            // ==== Declaring Variables ====
            const string sql = @"
                INSERT INTO RESERVERING_DETAIL (ReserveringID, CategorieID, Aantal, PrijsOpMoment)
                VALUES (@ReserveringID, @CategorieID, @Aantal, @PrijsOpMoment);
                SELECT LAST_INSERT_ID();";

            // ==== Start of Function ====
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@ReserveringID", detail.ReserveringID);
            command.Parameters.AddWithValue("@CategorieID", detail.CategorieID);
            command.Parameters.AddWithValue("@Aantal", detail.Aantal);
            command.Parameters.AddWithValue("@PrijsOpMoment", detail.PrijsOpMoment);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        // Haal detailregels van een reservering
        public async Task<List<ReserveringDetailDTO>> GetReservationDetailsAsync(int reserveringId)
        {
            // ==== Declaring Variables ====
            var details = new List<ReserveringDetailDTO>();
            const string sql = @"
                SELECT DetailID, ReserveringID, CategorieID, Aantal, PrijsOpMoment
                FROM RESERVERING_DETAIL
                WHERE ReserveringID = @ReserveringID";

            // ==== Start of Function ====
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@ReserveringID", reserveringId);

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                details.Add(new ReserveringDetailDTO
                {
                    DetailID = reader.GetInt32("DetailID"),
                    ReserveringID = reader.GetInt32("ReserveringID"),
                    CategorieID = reader.GetInt32("CategorieID"),
                    Aantal = reader.GetInt32("Aantal"),
                    PrijsOpMoment = reader.GetDecimal("PrijsOpMoment")
                });
            }

            return details;
        }

        // ============================================================
        // ==== GASTEN METHODS ====
        // ============================================================

        // Haal alle gasten
        public async Task<List<GastDTO>> GetAllGastenAsync()
        {
            // ==== Declaring Variables ====
            var gasten = new List<GastDTO>();
            const string sql = @"
                SELECT GastID, Naam, Email, Tel, Straat, Huisnr, Postcode, Plaats, Land, IBAN
                FROM GAST
                ORDER BY Naam";

            // ==== Start of Function ====
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                gasten.Add(MapToGastDTO(reader));
            }

            return gasten;
        }

        // Zoek gast op email
        public async Task<GastDTO?> GetGastByEmailAsync(string email)
        {
            // ==== Declaring Variables ====
            GastDTO? gast = null;
            const string sql = @"
                SELECT GastID, Naam, Email, Tel, Straat, Huisnr, Postcode, Plaats, Land, IBAN
                FROM GAST
                WHERE Email = @Email";

            // ==== Start of Function ====
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Email", email);

            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                gast = MapToGastDTO(reader);
            }

            return gast;
        }

        // Haal gast op via ID
        public async Task<GastDTO?> GetGastByIdAsync(int gastId)
        {
            // ==== Declaring Variables ====
            GastDTO? gast = null;
            const string sql = @"
                SELECT GastID, Naam, Email, Tel, Straat, Huisnr, Postcode, Plaats, Land, IBAN
                FROM GAST
                WHERE GastID = @GastID";

            // ==== Start of Function ====
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@GastID", gastId);

            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                gast = MapToGastDTO(reader);
            }

            return gast;
        }

        // Maak nieuwe gast, retourneer nieuw GastID
        public async Task<int> CreateGastAsync(GastDTO gast)
        {
            // ==== Declaring Variables ====
            const string sql = @"
                INSERT INTO GAST (Naam, Email, Tel, Straat, Huisnr, Postcode, Plaats, Land, IBAN)
                VALUES (@Naam, @Email, @Tel, @Straat, @Huisnr, @Postcode, @Plaats, @Land, @IBAN);
                SELECT LAST_INSERT_ID();";

            // ==== Start of Function ====
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Naam", gast.Naam);
            command.Parameters.AddWithValue("@Email", gast.Email);
            command.Parameters.AddWithValue("@Tel", (object?)gast.Tel ?? DBNull.Value);
            command.Parameters.AddWithValue("@Straat", gast.Straat);
            command.Parameters.AddWithValue("@Huisnr", gast.Huisnr);
            command.Parameters.AddWithValue("@Postcode", gast.Postcode);
            command.Parameters.AddWithValue("@Plaats", gast.Plaats);
            command.Parameters.AddWithValue("@Land", gast.Land);
            command.Parameters.AddWithValue("@IBAN", (object?)gast.IBAN ?? DBNull.Value);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        // Update gastgegevens
        public async Task<bool> UpdateGastAsync(GastDTO gast)
        {
            // ==== Declaring Variables ====
            const string sql = @"
                UPDATE GAST 
                SET Naam = @Naam, Email = @Email, Tel = @Tel, Straat = @Straat, 
                    Huisnr = @Huisnr, Postcode = @Postcode, Plaats = @Plaats, 
                    Land = @Land, IBAN = @IBAN
                WHERE GastID = @GastID";

            // ==== Start of Function ====
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@GastID", gast.GastID);
            command.Parameters.AddWithValue("@Naam", gast.Naam);
            command.Parameters.AddWithValue("@Email", gast.Email);
            command.Parameters.AddWithValue("@Tel", (object?)gast.Tel ?? DBNull.Value);
            command.Parameters.AddWithValue("@Straat", gast.Straat);
            command.Parameters.AddWithValue("@Huisnr", gast.Huisnr);
            command.Parameters.AddWithValue("@Postcode", gast.Postcode);
            command.Parameters.AddWithValue("@Plaats", gast.Plaats);
            command.Parameters.AddWithValue("@Land", gast.Land);
            command.Parameters.AddWithValue("@IBAN", (object?)gast.IBAN ?? DBNull.Value);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        // ============================================================
        // ==== GEBRUIKERS METHODS ====
        // ============================================================

        // Haal alle gebruikers
        public async Task<List<GebruikerDTO>> GetAllGebruikersAsync()
        {
            // ==== Declaring Variables ====
            var gebruikers = new List<GebruikerDTO>();
            const string sql = @"
                SELECT GebruikerID, GastID, Email, Rol
                FROM GEBRUIKER
                ORDER BY Email";

            // ==== Start of Function ====
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                gebruikers.Add(MapToGebruikerDTO(reader));
            }

            return gebruikers;
        }

        // Haal gebruiker op via ID
        public async Task<GebruikerDTO?> GetGebruikerByIdAsync(int gebruikerId)
        {
            // ==== Declaring Variables ====
            GebruikerDTO? gebruiker = null;
            const string sql = @"
                SELECT GebruikerID, GastID, Email, Rol
                FROM GEBRUIKER
                WHERE GebruikerID = @GebruikerID";

            // ==== Start of Function ====
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@GebruikerID", gebruikerId);

            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                gebruiker = MapToGebruikerDTO(reader);
            }

            return gebruiker;
        }

        // Zoek gebruiker op email
        public async Task<GebruikerDTO?> GetGebruikerByEmailAsync(string email)
        {
            // ==== Declaring Variables ====
            GebruikerDTO? gebruiker = null;
            const string sql = @"
                SELECT GebruikerID, GastID, Email, Rol
                FROM GEBRUIKER
                WHERE Email = @Email";

            // ==== Start of Function ====
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Email", email);

            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                gebruiker = MapToGebruikerDTO(reader);
            }

            return gebruiker;
        }

        // ============================================================
        // ==== TARIEVEN METHODS ====
        // ============================================================

        // Haal geldig tarief voor type/platform op datum
        // Controleert GeldigVan/GeldigTot
        public async Task<TariefDTO?> GetTariefAsync(int typeId, int platformId, DateTime datum)
        {
            // ==== Declaring Variables ====
            TariefDTO? tarief = null;
            const string sql = @"
                SELECT TariefID, TypeID, CategorieID, PlatformID, Prijs, TaxStatus, TaxTarief, GeldigVan, GeldigTot
                FROM TARIEF
                WHERE TypeID = @TypeID
                  AND (PlatformID = @PlatformID OR PlatformID IS NULL)
                  AND GeldigVan <= @Datum
                  AND (GeldigTot IS NULL OR GeldigTot >= @Datum)
                ORDER BY PlatformID DESC
                LIMIT 1";

            // ==== Start of Function ====
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@TypeID", typeId);
            command.Parameters.AddWithValue("@PlatformID", platformId);
            command.Parameters.AddWithValue("@Datum", datum);

            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                tarief = MapToTariefDTO(reader);
            }

            return tarief;
        }

        // Haal alle tarieven
        public async Task<List<TariefDTO>> GetAllTarievenAsync()
        {
            // ==== Declaring Variables ====
            var tarieven = new List<TariefDTO>();
            const string sql = @"
                SELECT TariefID, TypeID, CategorieID, PlatformID, Prijs, TaxStatus, TaxTarief, GeldigVan, GeldigTot
                FROM TARIEF
                ORDER BY TypeID, PlatformID";

            // ==== Start of Function ====
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                tarieven.Add(MapToTariefDTO(reader));
            }

            return tarieven;
        }

        // ============================================================
        // ==== TARIEF CATEGORIEËN METHODS ====
        // ============================================================

        // Haal alle tariefcategorieën
        public async Task<List<TariefCategorieDTO>> GetAllTariefCategoriesAsync()
        {
            // ==== Declaring Variables ====
            var categories = new List<TariefCategorieDTO>();
            const string sql = @"
                SELECT CategorieID, Naam
                FROM TARIEF_CATEGORIE
                ORDER BY CategorieID";

            // ==== Start of Function ====
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                categories.Add(new TariefCategorieDTO
                {
                    CategorieID = reader.GetInt32("CategorieID"),
                    Naam = reader.GetString("Naam")
                });
            }

            return categories;
        }

        // Haal tariefcategorie op via ID
        public async Task<TariefCategorieDTO?> GetTariefCategorieByIdAsync(int categorieId)
        {
            // ==== Declaring Variables ====
            TariefCategorieDTO? categorie = null;
            const string sql = @"
                SELECT CategorieID, Naam
                FROM TARIEF_CATEGORIE
                WHERE CategorieID = @CategorieID";

            // ==== Start of Function ====
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@CategorieID", categorieId);

            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                categorie = new TariefCategorieDTO
                {
                    CategorieID = reader.GetInt32("CategorieID"),
                    Naam = reader.GetString("Naam")
                };
            }

            return categorie;
        }

        // ============================================================
        // ==== PLATFORMEN METHODS ====
        // ============================================================

        // Haal alle platformen
        public async Task<List<PlatformDTO>> GetAllPlatformsAsync()
        {
            // ==== Declaring Variables ====
            var platforms = new List<PlatformDTO>();
            const string sql = @"
                SELECT PlatformID, Naam, CommissiePercentage
                FROM PLATFORM
                ORDER BY PlatformID";

            // ==== Start of Function ====
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                platforms.Add(MapToPlatformDTO(reader));
            }

            return platforms;
        }

        // Haal platform op via ID
        public async Task<PlatformDTO?> GetPlatformByIdAsync(int platformId)
        {
            // ==== Declaring Variables ====
            PlatformDTO? platform = null;
            const string sql = @"
                SELECT PlatformID, Naam, CommissiePercentage
                FROM PLATFORM
                WHERE PlatformID = @PlatformID";

            // ==== Start of Function ====
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@PlatformID", platformId);

            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                platform = MapToPlatformDTO(reader);
            }

            return platform;
        }

        // ============================================================
        // ==== ACCOMMODATIE TYPES METHODS ====
        // ============================================================

        // Haal alle accommodatie types
        public async Task<List<AccommodatieTypeDTO>> GetAllAccommodatieTypesAsync()
        {
            // ==== Declaring Variables ====
            var types = new List<AccommodatieTypeDTO>();
            const string sql = @"
                SELECT TypeID, Naam
                FROM ACCOMMODATIE_TYPE
                ORDER BY TypeID";

            // ==== Start of Function ====
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                types.Add(new AccommodatieTypeDTO
                {
                    TypeID = reader.GetInt32("TypeID"),
                    Naam = reader.GetString("Naam")
                });
            }

            return types;
        }

        // Haal accommodatie type op via ID
        public async Task<AccommodatieTypeDTO?> GetAccommodatieTypeByIdAsync(int typeId)
        {
            // ==== Declaring Variables ====
            AccommodatieTypeDTO? type = null;
            const string sql = @"
                SELECT TypeID, Naam
                FROM ACCOMMODATIE_TYPE
                WHERE TypeID = @TypeID";

            // ==== Start of Function ====
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@TypeID", typeId);

            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                type = new AccommodatieTypeDTO
                {
                    TypeID = reader.GetInt32("TypeID"),
                    Naam = reader.GetString("Naam")
                };
            }

            return type;
        }

        // ============================================================
        // ==== LOGBOEK METHODS (Audit Trail) ====
        // ============================================================

        // Maak log entry voor audit trail, retourneer nieuw ID
        public async Task<int> CreateLogEntryAsync(LogboekDTO logEntry)
        {
            // ==== Declaring Variables ====
            const string sql = @"
                INSERT INTO LOGBOEK (GebruikerID, Tijdstip, Actie, TabelNaam, RecordID, OudeWaarde, NieuweWaarde)
                VALUES (@GebruikerID, @Tijdstip, @Actie, @TabelNaam, @RecordID, @OudeWaarde, @NieuweWaarde);
                SELECT LAST_INSERT_ID();";

            // ==== Start of Function ====
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@GebruikerID", (object?)logEntry.GebruikerID ?? DBNull.Value);
            command.Parameters.AddWithValue("@Tijdstip", logEntry.Tijdstip);
            command.Parameters.AddWithValue("@Actie", logEntry.Actie);
            command.Parameters.AddWithValue("@TabelNaam", (object?)logEntry.TabelNaam ?? DBNull.Value);
            command.Parameters.AddWithValue("@RecordID", (object?)logEntry.RecordID ?? DBNull.Value);
            command.Parameters.AddWithValue("@OudeWaarde", (object?)logEntry.OudeWaarde ?? DBNull.Value);
            command.Parameters.AddWithValue("@NieuweWaarde", (object?)logEntry.NieuweWaarde ?? DBNull.Value);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        // Haal recente log entries (nieuwste eerst)
        public async Task<List<LogboekDTO>> GetRecentLogsAsync(int count = 50)
        {
            // ==== Declaring Variables ====
            var logs = new List<LogboekDTO>();
            const string sql = @"
                SELECT LogID, GebruikerID, Tijdstip, Actie, TabelNaam, RecordID, OudeWaarde, NieuweWaarde
                FROM LOGBOEK
                ORDER BY Tijdstip DESC
                LIMIT @Count";

            // ==== Start of Function ====
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Count", count);

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                logs.Add(MapToLogboekDTO(reader));
            }

            return logs;
        }

        // ============================================================
        // ==== PRIVATE MAPPING METHODS ====
        // ============================================================

        private static VerhuurEenheidDTO MapToVerhuurEenheidDTO(MySqlDataReader reader) => new()
        {
            EenheidID = reader.GetInt32("EenheidID"),
            Naam = reader.GetString("Naam"),
            TypeID = reader.GetInt32("TypeID"),
            MaxCapaciteit = reader.GetInt32("MaxCapaciteit"),
            ParentEenheidID = reader.IsDBNull(reader.GetOrdinal("ParentEenheidID")) ? null : reader.GetInt32("ParentEenheidID")
        };

        private static ReserveringDTO MapToReserveringDTO(MySqlDataReader reader) => new()
        {
            ReserveringID = reader.GetInt32("ReserveringID"),
            GastID = reader.GetInt32("GastID"),
            EenheidID = reader.GetInt32("EenheidID"),
            PlatformID = reader.GetInt32("PlatformID"),
            Startdatum = reader.GetDateTime("Startdatum"),
            Einddatum = reader.GetDateTime("Einddatum"),
            Status = reader.GetString("Status")
        };

        private static GastDTO MapToGastDTO(MySqlDataReader reader) => new()
        {
            GastID = reader.GetInt32("GastID"),
            Naam = reader.GetString("Naam"),
            Email = reader.GetString("Email"),
            Tel = reader.IsDBNull(reader.GetOrdinal("Tel")) ? null : reader.GetString("Tel"),
            Straat = reader.GetString("Straat"),
            Huisnr = reader.GetString("Huisnr"),
            Postcode = reader.GetString("Postcode"),
            Plaats = reader.GetString("Plaats"),
            Land = reader.GetString("Land"),
            IBAN = reader.IsDBNull(reader.GetOrdinal("IBAN")) ? null : reader.GetString("IBAN")
        };

        private static GebruikerDTO MapToGebruikerDTO(MySqlDataReader reader) => new()
        {
            GebruikerID = reader.GetInt32("GebruikerID"),
            GastID = reader.IsDBNull(reader.GetOrdinal("GastID")) ? null : reader.GetInt32("GastID"),
            Email = reader.GetString("Email"),
            Rol = reader.GetString("Rol")
        };

        private static TariefDTO MapToTariefDTO(MySqlDataReader reader) => new()
        {
            TariefID = reader.GetInt32("TariefID"),
            TypeID = reader.GetInt32("TypeID"),
            CategorieID = reader.GetInt32("CategorieID"),
            PlatformID = reader.IsDBNull(reader.GetOrdinal("PlatformID")) ? null : reader.GetInt32("PlatformID"),
            Prijs = reader.GetDecimal("Prijs"),
            TaxStatus = reader.GetBoolean("TaxStatus"),
            TaxTarief = reader.GetDecimal("TaxTarief"),
            GeldigVan = reader.GetDateTime("GeldigVan"),
            GeldigTot = reader.IsDBNull(reader.GetOrdinal("GeldigTot")) ? null : reader.GetDateTime("GeldigTot")
        };

        private static PlatformDTO MapToPlatformDTO(MySqlDataReader reader) => new()
        {
            PlatformID = reader.GetInt32("PlatformID"),
            Naam = reader.GetString("Naam"),
            CommissiePercentage = reader.GetDecimal("CommissiePercentage")
        };

        private static LogboekDTO MapToLogboekDTO(MySqlDataReader reader) => new()
        {
            LogID = reader.GetInt32("LogID"),
            GebruikerID = reader.IsDBNull(reader.GetOrdinal("GebruikerID")) ? null : reader.GetInt32("GebruikerID"),
            Tijdstip = reader.GetDateTime("Tijdstip"),
            Actie = reader.GetString("Actie"),
            TabelNaam = reader.IsDBNull(reader.GetOrdinal("TabelNaam")) ? null : reader.GetString("TabelNaam"),
            RecordID = reader.IsDBNull(reader.GetOrdinal("RecordID")) ? null : reader.GetInt32("RecordID"),
            OudeWaarde = reader.IsDBNull(reader.GetOrdinal("OudeWaarde")) ? null : reader.GetString("OudeWaarde"),
            NieuweWaarde = reader.IsDBNull(reader.GetOrdinal("NieuweWaarde")) ? null : reader.GetString("NieuweWaarde")
        };
    }
}
