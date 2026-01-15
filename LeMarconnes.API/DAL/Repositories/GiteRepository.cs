// ======== Imports ========
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySqlConnector;
using Microsoft.Extensions.Configuration;

// Internally
using LeMarconnes.Shared.DTOs;
using LeMarconnes.API.DAL.Interfaces;

// ======== Namespace ========
namespace LeMarconnes.API.DAL.Repositories {
    // Repository voor Gîte database operaties. Gebruikt ADO.NET met MySqlConnector.
    // Alle queries gebruiken parameterized statements (SQL injection preventie).
    public class GiteRepository : IGiteRepository {
        // ==== Constants ====
        private const string QUERY_RESERVERINGEN = @"
            SELECT 
                r.ReserveringID, r.GastID, r.EenheidID, r.PlatformID, r.Startdatum, r.Einddatum, r.Status,
                g.Naam as GastNaam, g.Email as GastEmail,
                e.Naam as EenheidNaam, e.MaxCapaciteit,
                p.Naam as PlatformNaam, p.CommissiePercentage
            FROM RESERVERING r
            JOIN GAST g ON r.GastID = g.GastID
            JOIN VERHUUR_EENHEID e ON r.EenheidID = e.EenheidID
            JOIN PLATFORM p ON r.PlatformID = p.PlatformID";
        private const string QUERY_UNITS = @"
            SELECT 
                ve.EenheidID, ve.Naam, ve.TypeID, ve.MaxCapaciteit, ve.ParentEenheidID,
                at.Naam as TypeNaam
            FROM VERHUUR_EENHEID ve
            JOIN ACCOMMODATIE_TYPE at ON ve.TypeID = at.TypeID
            WHERE ve.TypeID IN (1, 2)";
        private const string QUERY_DETAILS = @"
            SELECT rd.DetailID, rd.ReserveringID, rd.CategorieID, rd.Aantal, rd.PrijsOpMoment,
                   tc.Naam as CategorieNaam
            FROM RESERVERING_DETAIL rd
            JOIN TARIEF_CATEGORIE tc ON rd.CategorieID = tc.CategorieID";
        private const string QUERY_USERS = @"
            SELECT u.GebruikerID, u.GastID, u.Email, u.Rol,
                   g.Naam as GastNaam, g.Email as GastEmail
            FROM GEBRUIKER u
            LEFT JOIN GAST g ON u.GastID = g.GastID";
        private const string QUERY_TARIEVEN = @"
            SELECT 
                t.TariefID, t.TypeID, t.CategorieID, t.PlatformID, t.Prijs, t.TaxStatus, t.TaxTarief, t.GeldigVan, t.GeldigTot,
                at.Naam as TypeNaam,
                tc.Naam as CategorieNaam,
                p.Naam as PlatformNaam
            FROM TARIEF t
            JOIN ACCOMMODATIE_TYPE at ON t.TypeID = at.TypeID
            JOIN TARIEF_CATEGORIE tc ON t.CategorieID = tc.CategorieID
            LEFT JOIN PLATFORM p ON t.PlatformID = p.PlatformID"; // Left join, want Platform is nullable
        
        // ==== Properties ====
        private readonly string _connectionString;

        // ==== Constructor ====
        public GiteRepository(IConfiguration configuration) {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found in appsettings.json");
        }

        // ==== Methods ====
        // Private (Helpers)
        private async Task<MySqlConnection> GetConnectionAsync() {
            var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            return connection;
        }

        // Private (Mappers)
        private static VerhuurEenheidDTO MapUnitFromReader(MySqlDataReader reader) {
            var unit = new VerhuurEenheidDTO {
                EenheidID = reader.GetInt32("EenheidID"),
                Naam = reader.GetString("Naam"),
                TypeID = reader.GetInt32("TypeID"),
                MaxCapaciteit = reader.GetInt32("MaxCapaciteit"),
                ParentEenheidID = reader.IsDBNull(reader.GetOrdinal("ParentEenheidID")) ? null : reader.GetInt32("ParentEenheidID")
            };
            // OO Fill
            unit.Type = new AccommodatieTypeDTO(unit.TypeID, reader.GetString("TypeNaam"));
            return unit;
        }
        private static ReserveringDTO MapReserveringFullFromReader(MySqlDataReader reader) {
            var res = new ReserveringDTO {
                ReserveringID = reader.GetInt32("ReserveringID"),
                GastID = reader.GetInt32("GastID"),
                EenheidID = reader.GetInt32("EenheidID"),
                PlatformID = reader.GetInt32("PlatformID"),
                Startdatum = reader.GetDateTime("Startdatum"),
                Einddatum = reader.GetDateTime("Einddatum"),
                Status = reader.GetString("Status")
            };

            // 1. Fill Gast Object
            res.Gast = new GastDTO {
                GastID = res.GastID,
                Naam = reader.GetString("GastNaam"),
                Email = reader.GetString("GastEmail")
                // Note: We don't fetch Address/Tel here to keep query light, unless requested.
            };

            // 2. Fill Platform Object
            res.Platform = new PlatformDTO {
                PlatformID = res.PlatformID,
                Naam = reader.GetString("PlatformNaam"),
                CommissiePercentage = reader.GetDecimal("CommissiePercentage")
            };

            // 3. Fill Eenheid Object (Basic info)
            res.Eenheid = new VerhuurEenheidDTO {
                EenheidID = res.EenheidID,
                Naam = reader.GetString("EenheidNaam"),
                MaxCapaciteit = reader.GetInt32("MaxCapaciteit")
                // Note: We don't stitch Parents/Children here, just the unit itself
            };

            return res;
        }
        private static GastDTO MapGastFromReader(MySqlDataReader reader) => new() {
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
        private static void AddGastParameters(MySqlCommand cmd, GastDTO gast) {
            cmd.Parameters.AddWithValue("@Naam", gast.Naam);
            cmd.Parameters.AddWithValue("@Email", gast.Email);
            cmd.Parameters.AddWithValue("@Tel", (object?)gast.Tel ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Straat", gast.Straat);
            cmd.Parameters.AddWithValue("@Huisnr", gast.Huisnr);
            cmd.Parameters.AddWithValue("@Postcode", gast.Postcode);
            cmd.Parameters.AddWithValue("@Plaats", gast.Plaats);
            cmd.Parameters.AddWithValue("@Land", gast.Land);
            cmd.Parameters.AddWithValue("@IBAN", (object?)gast.IBAN ?? DBNull.Value);
        }
        private static GebruikerDTO MapGebruikerFullFromReader(MySqlDataReader reader) {
            var user = new GebruikerDTO {
                GebruikerID = reader.GetInt32("GebruikerID"),
                GastID = reader.IsDBNull(reader.GetOrdinal("GastID")) ? null : reader.GetInt32("GastID"),
                Email = reader.GetString("Email"),
                Rol = reader.GetString("Rol")
            };

            // OO Vul: Als er een GastID is, vul het Gast object (deels)
            if (user.GastID.HasValue) {
                user.Gast = new GastDTO {
                    GastID = user.GastID.Value,
                    Naam = reader.GetString("GastNaam"),
                    Email = reader.GetString("GastEmail")
                };
            }
            return user;
        }
        private static TariefDTO MapTariefFullFromReader(MySqlDataReader reader) {
            var t = new TariefDTO {
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

            // OO Vullen (Joins)
            t.Type = new AccommodatieTypeDTO(t.TypeID, reader.GetString("TypeNaam"));
            t.Categorie = new TariefCategorieDTO(t.CategorieID, reader.GetString("CategorieNaam"));

            if (t.PlatformID.HasValue) {
                t.Platform = new PlatformDTO(t.PlatformID.Value, reader.GetString("PlatformNaam"), 0); // % niet in query, boeit hier minder
            }

            return t;
        }
        private static PlatformDTO MapPlatformFromReader(MySqlDataReader reader) => new() {
            PlatformID = reader.GetInt32("PlatformID"),
            Naam = reader.GetString("Naam"),
            CommissiePercentage = reader.GetDecimal("CommissiePercentage")
        };

        // Public
        // ==== VERHUUR_EENHEID METHODS ====
        public async Task<List<VerhuurEenheidDTO>> GetAllGiteUnitsAsync() {
            var units = new List<VerhuurEenheidDTO>();

            await using var conn = await GetConnectionAsync();
            await using var cmd = new MySqlCommand($"{QUERY_UNITS} ORDER BY ve.EenheidID", conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync()) {
                units.Add(MapUnitFromReader(reader));
            }

            // OO Stitching: Connect Parents and Children in memory
            var unitDict = units.ToDictionary(u => u.EenheidID);
            foreach (var unit in units) {
                if (unit.ParentEenheidID.HasValue && unitDict.TryGetValue(unit.ParentEenheidID.Value, out var parentUnit)) {
                    unit.ParentEenheid = parentUnit;
                    parentUnit.ChildEenheden.Add(unit);
                }
            }

            return units;
        }
        public async Task<VerhuurEenheidDTO?> GetUnitByIdAsync(int eenheidId) {
            // We reuse GetAll to ensure hierarchy is stitched correctly
            var allUnits = await GetAllGiteUnitsAsync();
            return allUnits.FirstOrDefault(u => u.EenheidID == eenheidId);
        }
        public async Task<List<VerhuurEenheidDTO>> GetChildUnitsAsync(int parentId) {
            var allUnits = await GetAllGiteUnitsAsync();
            var parent = allUnits.FirstOrDefault(u => u.EenheidID == parentId);
            return parent?.ChildEenheden ?? new List<VerhuurEenheidDTO>();
        }

        // ==== RESERVERING METHODS ====
        public async Task<List<ReserveringDTO>> GetAllReserveringenAsync() {
            var list = new List<ReserveringDTO>();

            await using var conn = await GetConnectionAsync();
            // Reuse base query + Order By
            await using var cmd = new MySqlCommand($"{QUERY_RESERVERINGEN} ORDER BY r.Startdatum DESC", conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync()) {
                list.Add(MapReserveringFullFromReader(reader));
            }
            return list;
        }
        public async Task<ReserveringDTO?> GetReserveringByIdAsync(int reserveringId) {
            ReserveringDTO? res = null;

            await using var conn = await GetConnectionAsync();
            await using var cmd = new MySqlCommand($"{QUERY_RESERVERINGEN} WHERE r.ReserveringID = @ID", conn);
            cmd.Parameters.AddWithValue("@ID", reserveringId);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync()) {
                res = MapReserveringFullFromReader(reader);
            }
            return res;
        }
        public async Task<List<ReserveringDTO>> GetReservationsByDateRangeAsync(DateTime startDatum, DateTime eindDatum) {
            var list = new List<ReserveringDTO>();

            await using var conn = await GetConnectionAsync();
            // Note: Overlap logic: (Start < ReqEnd) AND (End > ReqStart)
            string sql = $"{QUERY_RESERVERINGEN} WHERE r.Startdatum < @Eind AND r.Einddatum > @Start AND r.Status != 'Geannuleerd'";

            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Start", startDatum);
            cmd.Parameters.AddWithValue("@Eind", eindDatum);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) {
                list.Add(MapReserveringFullFromReader(reader));
            }
            return list;
        }
        public async Task<List<ReserveringDTO>> GetReservationsForUnitAsync(int eenheidId, DateTime startDatum, DateTime eindDatum) {
            var list = new List<ReserveringDTO>();

            await using var conn = await GetConnectionAsync();
            string sql = $"{QUERY_RESERVERINGEN} WHERE r.EenheidID = @UnitID AND r.Startdatum < @Eind AND r.Einddatum > @Start AND r.Status != 'Geannuleerd'";

            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UnitID", eenheidId);
            cmd.Parameters.AddWithValue("@Start", startDatum);
            cmd.Parameters.AddWithValue("@Eind", eindDatum);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) {
                list.Add(MapReserveringFullFromReader(reader));
            }
            return list;
        }
        public async Task<List<ReserveringDTO>> GetReservationsForGastAsync(int gastId) {
            var list = new List<ReserveringDTO>();

            await using var conn = await GetConnectionAsync();
            await using var cmd = new MySqlCommand($"{QUERY_RESERVERINGEN} WHERE r.GastID = @GastID ORDER BY r.Startdatum DESC", conn);
            cmd.Parameters.AddWithValue("@GastID", gastId);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) {
                list.Add(MapReserveringFullFromReader(reader));
            }
            return list;
        }
        public async Task<int> CreateReservationAsync(ReserveringDTO reservering) {
            const string sql = @"
                INSERT INTO RESERVERING (GastID, EenheidID, PlatformID, Startdatum, Einddatum, Status)
                VALUES (@GastID, @EenheidID, @PlatformID, @Startdatum, @Einddatum, @Status);
                SELECT LAST_INSERT_ID();";

            await using var conn = await GetConnectionAsync();
            await using var cmd = new MySqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@GastID", reservering.GastID);
            cmd.Parameters.AddWithValue("@EenheidID", reservering.EenheidID);
            cmd.Parameters.AddWithValue("@PlatformID", reservering.PlatformID);
            cmd.Parameters.AddWithValue("@Startdatum", reservering.Startdatum);
            cmd.Parameters.AddWithValue("@Einddatum", reservering.Einddatum);
            cmd.Parameters.AddWithValue("@Status", reservering.Status);

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }
        public async Task<bool> UpdateReservationStatusAsync(int reserveringId, string status) {
            const string sql = "UPDATE RESERVERING SET Status = @Status WHERE ReserveringID = @ID";

            await using var conn = await GetConnectionAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ID", reserveringId);
            cmd.Parameters.AddWithValue("@Status", status);

            return await cmd.ExecuteNonQueryAsync() > 0;
        }
        public async Task<bool> DeleteReservationAsync(int reserveringId) {
            // Note: DB Constraints (Cascade Delete) should handle details normally,
            // but explicitly deleting creates clarity.
            const string sql = "DELETE FROM RESERVERING WHERE ReserveringID = @ID";

            await using var conn = await GetConnectionAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ID", reserveringId);

            return await cmd.ExecuteNonQueryAsync() > 0;
        }
        public async Task<int> CreateReservationDetailAsync(ReserveringDetailDTO detail) {
            const string sql = @"
                INSERT INTO RESERVERING_DETAIL (ReserveringID, CategorieID, Aantal, PrijsOpMoment)
                VALUES (@ResID, @CatID, @Aantal, @Prijs);
                SELECT LAST_INSERT_ID();";

            await using var conn = await GetConnectionAsync();
            await using var cmd = new MySqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@ResID", detail.ReserveringID);
            cmd.Parameters.AddWithValue("@CatID", detail.CategorieID);
            cmd.Parameters.AddWithValue("@Aantal", detail.Aantal);
            cmd.Parameters.AddWithValue("@Prijs", detail.PrijsOpMoment);

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }
        public async Task<List<ReserveringDetailDTO>> GetReservationDetailsAsync(int reserveringId) {
            var details = new List<ReserveringDetailDTO>();

            await using var conn = await GetConnectionAsync();
            // Gebruikt QUERY_DETAILS om ook de CategorieNaam op te halen
            await using var cmd = new MySqlCommand($"{QUERY_DETAILS} WHERE rd.ReserveringID = @ID", conn);
            cmd.Parameters.AddWithValue("@ID", reserveringId);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) {
                var detail = new ReserveringDetailDTO {
                    DetailID = reader.GetInt32("DetailID"),
                    ReserveringID = reader.GetInt32("ReserveringID"),
                    CategorieID = reader.GetInt32("CategorieID"),
                    Aantal = reader.GetInt32("Aantal"),
                    PrijsOpMoment = reader.GetDecimal("PrijsOpMoment")
                };

                // OO Vul: De categorie
                detail.Categorie = new TariefCategorieDTO(detail.CategorieID, reader.GetString("CategorieNaam"));

                details.Add(detail);
            }
            return details;
        }

        // ==== GAST METHODS ====
        public async Task<List<GastDTO>> GetAllGastenAsync() {
            var gasten = new List<GastDTO>();
            const string sql = "SELECT * FROM GAST ORDER BY Naam"; // Simple select is fine here

            await using var conn = await GetConnectionAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync()) gasten.Add(MapGastFromReader(reader));
            return gasten;
        }
        public async Task<GastDTO?> GetGastByIdAsync(int gastId) {
            await using var conn = await GetConnectionAsync();
            await using var cmd = new MySqlCommand("SELECT * FROM GAST WHERE GastID = @ID", conn);
            cmd.Parameters.AddWithValue("@ID", gastId);

            await using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapGastFromReader(reader) : null;
        }
        public async Task<GastDTO?> GetGastByEmailAsync(string email) {
            await using var conn = await GetConnectionAsync();
            await using var cmd = new MySqlCommand("SELECT * FROM GAST WHERE Email = @Email", conn);
            cmd.Parameters.AddWithValue("@Email", email);

            await using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapGastFromReader(reader) : null;
        }
        public async Task<bool> UpdateGastAsync(GastDTO gast) {
            const string sql = @"
                UPDATE GAST 
                SET Naam = @Naam, Email = @Email, Tel = @Tel, Straat = @Straat, 
                    Huisnr = @Huisnr, Postcode = @Postcode, Plaats = @Plaats, 
                    Land = @Land, IBAN = @IBAN
                WHERE GastID = @GastID";

            await using var conn = await GetConnectionAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@GastID", gast.GastID);
            AddGastParameters(cmd, gast);

            return await cmd.ExecuteNonQueryAsync() > 0;
        }
        public async Task<int> CreateGastAsync(GastDTO gast) {
            const string sql = @"
                INSERT INTO GAST (Naam, Email, Tel, Straat, Huisnr, Postcode, Plaats, Land, IBAN)
                VALUES (@Naam, @Email, @Tel, @Straat, @Huisnr, @Postcode, @Plaats, @Land, @IBAN);
                SELECT LAST_INSERT_ID();";

            await using var conn = await GetConnectionAsync();
            await using var cmd = new MySqlCommand(sql, conn);

            // We gebruiken de helper hieronder om duplicatie met UpdateGastAsync te voorkomen
            AddGastParameters(cmd, gast);

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        // ==== GEBRUIKER METHODS ====
        public async Task<List<GebruikerDTO>> GetAllGebruikersAsync() {
            var list = new List<GebruikerDTO>();
            await using var conn = await GetConnectionAsync();
            await using var cmd = new MySqlCommand($"{QUERY_USERS} ORDER BY u.Email", conn);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) list.Add(MapGebruikerFullFromReader(reader));
            return list;
        }
        public async Task<GebruikerDTO?> GetGebruikerByIdAsync(int gebruikerId) {
            await using var conn = await GetConnectionAsync();
            await using var cmd = new MySqlCommand($"{QUERY_USERS} WHERE u.GebruikerID = @ID", conn);
            cmd.Parameters.AddWithValue("@ID", gebruikerId);

            await using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapGebruikerFullFromReader(reader) : null;
        }
        public async Task<GebruikerDTO?> GetGebruikerByEmailAsync(string email) {
            await using var conn = await GetConnectionAsync();
            await using var cmd = new MySqlCommand($"{QUERY_USERS} WHERE u.Email = @Email", conn);
            cmd.Parameters.AddWithValue("@Email", email);

            await using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapGebruikerFullFromReader(reader) : null;
        }

        // ==== TARIEF METHODS ====
        public async Task<TariefDTO?> GetTariefAsync(int typeId, int platformId, DateTime datum) {
            TariefDTO? tarief = null;
            // Let op de logica: GeldigVan <= Nu EN (GeldigTot IS NULL OF GeldigTot >= Nu)
            string sql = $@"{QUERY_TARIEVEN} 
                WHERE t.TypeID = @TypeID
                  AND (t.PlatformID = @PlatformID OR t.PlatformID IS NULL)
                  AND t.GeldigVan <= @Datum
                  AND (t.GeldigTot IS NULL OR t.GeldigTot >= @Datum)
                ORDER BY t.PlatformID DESC -- Specifiek platform (niet-null) krijgt voorrang
                LIMIT 1";

            await using var conn = await GetConnectionAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@TypeID", typeId);
            cmd.Parameters.AddWithValue("@PlatformID", platformId);
            cmd.Parameters.AddWithValue("@Datum", datum);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync()) {
                tarief = MapTariefFullFromReader(reader);
            }
            return tarief;
        }
        public async Task<List<TariefDTO>> GetAllTarievenAsync() {
            var list = new List<TariefDTO>();
            await using var conn = await GetConnectionAsync();
            await using var cmd = new MySqlCommand($"{QUERY_TARIEVEN} ORDER BY t.TypeID, t.PlatformID", conn);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) list.Add(MapTariefFullFromReader(reader));
            return list;
        }
        public async Task<List<TariefCategorieDTO>> GetAllTariefCategoriesAsync() {
            var list = new List<TariefCategorieDTO>();
            await using var conn = await GetConnectionAsync();
            await using var cmd = new MySqlCommand("SELECT * FROM TARIEF_CATEGORIE ORDER BY CategorieID", conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync()) {
                list.Add(new TariefCategorieDTO(reader.GetInt32("CategorieID"), reader.GetString("Naam")));
            }
            return list;
        }
        public async Task<TariefCategorieDTO?> GetTariefCategorieByIdAsync(int id) {
            await using var conn = await GetConnectionAsync();
            await using var cmd = new MySqlCommand("SELECT * FROM TARIEF_CATEGORIE WHERE CategorieID = @ID", conn);
            cmd.Parameters.AddWithValue("@ID", id);
            await using var reader = await cmd.ExecuteReaderAsync();

            return await reader.ReadAsync() ? new TariefCategorieDTO(reader.GetInt32("CategorieID"), reader.GetString("Naam")) : null;
        }

        // ==== PLATFORM METHODS ====
        public async Task<List<PlatformDTO>> GetAllPlatformsAsync() {
            var list = new List<PlatformDTO>();
            await using var conn = await GetConnectionAsync();
            await using var cmd = new MySqlCommand("SELECT * FROM PLATFORM ORDER BY PlatformID", conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync()) list.Add(MapPlatformFromReader(reader));
            return list;
        }
        public async Task<PlatformDTO?> GetPlatformByIdAsync(int id) {
            await using var conn = await GetConnectionAsync();
            await using var cmd = new MySqlCommand("SELECT * FROM PLATFORM WHERE PlatformID = @ID", conn);
            cmd.Parameters.AddWithValue("@ID", id);
            await using var reader = await cmd.ExecuteReaderAsync();

            return await reader.ReadAsync() ? MapPlatformFromReader(reader) : null;
        }

        // ==== ACCOMMODATIE TYPE METHODS ====
        public async Task<List<AccommodatieTypeDTO>> GetAllAccommodatieTypesAsync() {
            var list = new List<AccommodatieTypeDTO>();
            await using var conn = await GetConnectionAsync();
            await using var cmd = new MySqlCommand("SELECT * FROM ACCOMMODATIE_TYPE ORDER BY TypeID", conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync()) {
                list.Add(new AccommodatieTypeDTO(reader.GetInt32("TypeID"), reader.GetString("Naam")));
            }
            return list;
        }
        public async Task<AccommodatieTypeDTO?> GetAccommodatieTypeByIdAsync(int id) {
            await using var conn = await GetConnectionAsync();
            await using var cmd = new MySqlCommand("SELECT * FROM ACCOMMODATIE_TYPE WHERE TypeID = @ID", conn);
            cmd.Parameters.AddWithValue("@ID", id);
            await using var reader = await cmd.ExecuteReaderAsync();

            return await reader.ReadAsync() ? new AccommodatieTypeDTO(reader.GetInt32("TypeID"), reader.GetString("Naam")) : null;
        }

        // ==== LOGBOEK METHODS ====
        public async Task<int> CreateLogEntryAsync(LogboekDTO log) {
            const string sql = @"
                INSERT INTO LOGBOEK (GebruikerID, Tijdstip, Actie, TabelNaam, RecordID, OudeWaarde, NieuweWaarde)
                VALUES (@GebruikerID, @Tijdstip, @Actie, @TabelNaam, @RecordID, @OudeWaarde, @NieuweWaarde);
                SELECT LAST_INSERT_ID();";

            await using var conn = await GetConnectionAsync();
            await using var cmd = new MySqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@GebruikerID", (object?)log.GebruikerID ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Tijdstip", log.Tijdstip);
            cmd.Parameters.AddWithValue("@Actie", log.Actie);
            cmd.Parameters.AddWithValue("@TabelNaam", (object?)log.TabelNaam ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@RecordID", (object?)log.RecordID ?? DBNull.Value); // Let op: command -> cmd
            cmd.Parameters.AddWithValue("@OudeWaarde", (object?)log.OudeWaarde ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@NieuweWaarde", (object?)log.NieuweWaarde ?? DBNull.Value);

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }
        public async Task<List<LogboekDTO>> GetRecentLogsAsync(int count = 50) {
            var list = new List<LogboekDTO>();
            // Optioneel: Je zou hier ook JOIN GEBRUIKER kunnen doen om te zien WIE het deed
            await using var conn = await GetConnectionAsync();
            await using var cmd = new MySqlCommand("SELECT * FROM LOGBOEK ORDER BY Tijdstip DESC LIMIT @Count", conn);
            cmd.Parameters.AddWithValue("@Count", count);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) {
                list.Add(new LogboekDTO {
                    LogID = reader.GetInt32("LogID"),
                    GebruikerID = reader.IsDBNull(reader.GetOrdinal("GebruikerID")) ? null : reader.GetInt32("GebruikerID"),
                    Tijdstip = reader.GetDateTime("Tijdstip"),
                    Actie = reader.GetString("Actie"),
                    TabelNaam = reader.IsDBNull(reader.GetOrdinal("TabelNaam")) ? null : reader.GetString("TabelNaam"),
                    RecordID = reader.IsDBNull(reader.GetOrdinal("RecordID")) ? null : reader.GetInt32("RecordID"),
                    OudeWaarde = reader.IsDBNull(reader.GetOrdinal("OudeWaarde")) ? null : reader.GetString("OudeWaarde"),
                    NieuweWaarde = reader.IsDBNull(reader.GetOrdinal("NieuweWaarde")) ? null : reader.GetString("NieuweWaarde")
                });
            }
            return list;
        }
    }
}