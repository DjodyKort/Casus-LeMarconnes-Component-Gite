// ======== Imports ========
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LeMarconnes.Shared.DTOs;

// ======== Namespace ========
namespace LeMarconnes.API.DAL.Interfaces
{
    /// <summary>
    /// Interface voor de Gîte Repository.
    /// Definieert het contract voor alle database operaties
    /// gerelateerd aan de Gîte module.
    /// 
    /// Dit is onderdeel van de DAL (Data Access Layer).
    /// 
    /// WAAROM EEN INTERFACE?
    /// 1. Dependency Injection: De controller vraagt IGiteRepository,
    ///    niet de concrete implementatie. Dit maakt het makkelijk om
    ///    later een andere implementatie te gebruiken (bijv. voor testing).
    /// 2. Testbaarheid: Je kunt een mock repository maken voor unit tests.
    /// 3. Losse koppeling: De controller weet niet HOE data opgehaald wordt,
    ///    alleen WAT er opgehaald kan worden.
    /// 
    /// ASYNC/AWAIT:
    /// Alle methods zijn async (Task) omdat database operaties I/O-bound zijn.
    /// Dit voorkomt dat de server blokkeert tijdens database queries.
    /// </summary>
    public interface IGiteRepository
    {
        // ============================================================
        // ==== VERHUUR EENHEDEN ====
        // Methods voor het ophalen van Gîte eenheden
        // ============================================================
        
        /// <summary>Haalt alle Gîte eenheden op (TypeID 1 en 2).</summary>
        Task<List<VerhuurEenheidDTO>> GetAllGiteUnitsAsync();
        
        /// <summary>Haalt een specifieke eenheid op basis van ID.</summary>
        Task<VerhuurEenheidDTO?> GetUnitByIdAsync(int eenheidId);
        
        /// <summary>Haalt alle children op voor een parent eenheid.</summary>
        Task<List<VerhuurEenheidDTO>> GetChildUnitsAsync(int parentId);

        // ============================================================
        // ==== RESERVERINGEN ====
        // CRUD operaties voor reserveringen
        // ============================================================
        
        /// <summary>Haalt alle reserveringen op, nieuwste eerst.</summary>
        Task<List<ReserveringDTO>> GetAllReserveringenAsync();
        
        /// <summary>Haalt een specifieke reservering op basis van ID.</summary>
        Task<ReserveringDTO?> GetReserveringByIdAsync(int reserveringId);
        
        /// <summary>Haalt alle overlappende reserveringen op voor een periode.</summary>
        Task<List<ReserveringDTO>> GetReservationsByDateRangeAsync(DateTime startDatum, DateTime eindDatum);
        
        /// <summary>Haalt reserveringen op voor een eenheid in een periode.</summary>
        Task<List<ReserveringDTO>> GetReservationsForUnitAsync(int eenheidId, DateTime startDatum, DateTime eindDatum);
        
        /// <summary>Haalt alle reserveringen op voor een gast.</summary>
        Task<List<ReserveringDTO>> GetReservationsForGastAsync(int gastId);
        
        /// <summary>Maakt een nieuwe reservering aan, retourneert het nieuwe ID.</summary>
        Task<int> CreateReservationAsync(ReserveringDTO reservering);
        
        /// <summary>Wijzigt de status van een reservering.</summary>
        Task<bool> UpdateReservationStatusAsync(int reserveringId, string status);
        
        /// <summary>Verwijdert een reservering permanent.</summary>
        Task<bool> DeleteReservationAsync(int reserveringId);
        
        /// <summary>Maakt een reservering detail regel aan (kostenpost).</summary>
        Task<int> CreateReservationDetailAsync(ReserveringDetailDTO detail);
        
        /// <summary>Haalt alle detail regels op voor een reservering.</summary>
        Task<List<ReserveringDetailDTO>> GetReservationDetailsAsync(int reserveringId);

        // ============================================================
        // ==== GASTEN ====
        // CRUD operaties voor gasten
        // ============================================================
        
        /// <summary>Haalt alle gasten op, gesorteerd op naam.</summary>
        Task<List<GastDTO>> GetAllGastenAsync();
        
        /// <summary>Zoekt een gast op basis van email (uniek).</summary>
        Task<GastDTO?> GetGastByEmailAsync(string email);
        
        /// <summary>Haalt een gast op basis van ID.</summary>
        Task<GastDTO?> GetGastByIdAsync(int gastId);
        
        /// <summary>Maakt een nieuwe gast aan, retourneert het nieuwe ID.</summary>
        Task<int> CreateGastAsync(GastDTO gast);
        
        /// <summary>Wijzigt de gegevens van een gast.</summary>
        Task<bool> UpdateGastAsync(GastDTO gast);

        // ============================================================
        // ==== GEBRUIKERS ====
        // Read-only operaties voor gebruikers (eigenaren)
        // ============================================================
        
        /// <summary>Haalt alle gebruikers op.</summary>
        Task<List<GebruikerDTO>> GetAllGebruikersAsync();
        
        /// <summary>Haalt een gebruiker op basis van ID.</summary>
        Task<GebruikerDTO?> GetGebruikerByIdAsync(int gebruikerId);
        
        /// <summary>Zoekt een gebruiker op basis van email.</summary>
        Task<GebruikerDTO?> GetGebruikerByEmailAsync(string email);

        // ============================================================
        // ==== TARIEVEN ====
        // Read-only operaties voor tarieven
        // ============================================================
        
        /// <summary>Haalt het geldige tarief op voor een type/platform combinatie.</summary>
        Task<TariefDTO?> GetTariefAsync(int typeId, int platformId, DateTime datum);
        
        /// <summary>Haalt alle tarieven op.</summary>
        Task<List<TariefDTO>> GetAllTarievenAsync();

        // ============================================================
        // ==== TARIEF CATEGORIEËN ====
        // Read-only operaties voor tarief categorieën (lookup)
        // ============================================================
        
        /// <summary>Haalt alle tarief categorieën op.</summary>
        Task<List<TariefCategorieDTO>> GetAllTariefCategoriesAsync();
        
        /// <summary>Haalt een tarief categorie op basis van ID.</summary>
        Task<TariefCategorieDTO?> GetTariefCategorieByIdAsync(int categorieId);

        // ============================================================
        // ==== PLATFORMEN ====
        // Read-only operaties voor platformen (lookup)
        // ============================================================
        
        /// <summary>Haalt alle platformen op.</summary>
        Task<List<PlatformDTO>> GetAllPlatformsAsync();
        
        /// <summary>Haalt een platform op basis van ID.</summary>
        Task<PlatformDTO?> GetPlatformByIdAsync(int platformId);

        // ============================================================
        // ==== ACCOMMODATIE TYPES ====
        // Read-only operaties voor accommodatie types (lookup)
        // ============================================================
        
        /// <summary>Haalt alle accommodatie types op.</summary>
        Task<List<AccommodatieTypeDTO>> GetAllAccommodatieTypesAsync();
        
        /// <summary>Haalt een accommodatie type op basis van ID.</summary>
        Task<AccommodatieTypeDTO?> GetAccommodatieTypeByIdAsync(int typeId);

        // ============================================================
        // ==== LOGBOEK (AUDIT TRAIL) ====
        // Operaties voor het logboek/audit trail
        // ============================================================
        
        /// <summary>Maakt een nieuwe log entry aan.</summary>
        Task<int> CreateLogEntryAsync(LogboekDTO logEntry);
        
        /// <summary>Haalt de meest recente log entries op.</summary>
        Task<List<LogboekDTO>> GetRecentLogsAsync(int count = 50);
    }
}
