// ======== Imports ========
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LeMarconnes.Shared.DTOs;

// ======== Namespace ========
namespace LeMarconnes.API.DAL.Interfaces
{
    // Interface voor repository operations van de Gîte module.
    // Kort, menselijk beschreven — details staan in implementatie.
    public interface IGiteRepository
    {

        // ==== VERHUUR EENHEDEN ====

        
        // Haal alle Gîte-eenheden (types 1 en 2)
        Task<List<VerhuurEenheidDTO>> GetAllGiteUnitsAsync();
        
        // Haal een eenheid op via ID
        Task<VerhuurEenheidDTO?> GetUnitByIdAsync(int eenheidId);
        
        // Haal children voor een parent eenheid
        Task<List<VerhuurEenheidDTO>> GetChildUnitsAsync(int parentId);


        // ==== RESERVERINGEN ====
        // Haal alle reserveringen (nieuwste eerst)
        Task<List<ReserveringDTO>> GetAllReserveringenAsync();
        
        // Haal reservering op via ID
        Task<ReserveringDTO?> GetReserveringByIdAsync(int reserveringId);
        
        // Haal reserveringen die overlappen met een periode
        Task<List<ReserveringDTO>> GetReservationsByDateRangeAsync(DateTime startDatum, DateTime eindDatum);
        
        // Haal reserveringen voor een specifieke eenheid binnen een periode
        Task<List<ReserveringDTO>> GetReservationsForUnitAsync(int eenheidId, DateTime startDatum, DateTime eindDatum);
        
        // Haal alle reserveringen van een gast
        Task<List<ReserveringDTO>> GetReservationsForGastAsync(int gastId);
        
        // Maak een nieuwe reservering, retourneer nieuw ID
        Task<int> CreateReservationAsync(ReserveringDTO reservering);
        
        // Update status van een reservering
        Task<bool> UpdateReservationStatusAsync(int reserveringId, string status);
        
        // Verwijder een reservering
        Task<bool> DeleteReservationAsync(int reserveringId);
        
        // Voeg een detailregel toe aan een reservering
        Task<int> CreateReservationDetailAsync(ReserveringDetailDTO detail);
        
        // Haal alle detailregels van een reservering
        Task<List<ReserveringDetailDTO>> GetReservationDetailsAsync(int reserveringId);


        // ==== GASTEN ====
        // Haal alle gasten (gesorteerd op naam)
        Task<List<GastDTO>> GetAllGastenAsync();
        
        // Zoek gast op email
        Task<GastDTO?> GetGastByEmailAsync(string email);
        
        // Haal gast op via ID
        Task<GastDTO?> GetGastByIdAsync(int gastId);
        
        // Maak nieuwe gast, retourneer nieuw ID
        Task<int> CreateGastAsync(GastDTO gast);
        
        // Update gastgegevens
        Task<bool> UpdateGastAsync(GastDTO gast);

        // ==== GEBRUIKERS ====       
        // Haal alle gebruikers
        Task<List<GebruikerDTO>> GetAllGebruikersAsync();
        
        // Haal gebruiker op via ID
        Task<GebruikerDTO?> GetGebruikerByIdAsync(int gebruikerId);
        
        // Zoek gebruiker op email
        Task<GebruikerDTO?> GetGebruikerByEmailAsync(string email);


        // ==== TARIEVEN ====
        // Haal geldig tarief voor type/platform op een gegeven datum
        Task<TariefDTO?> GetTariefAsync(int typeId, int platformId, DateTime datum);
        
        // Haal alle tarieven
        Task<List<TariefDTO>> GetAllTarievenAsync();


        // ==== TARIEF CATEGORIEËN ====
        // Haal alle tariefcategorieën
        Task<List<TariefCategorieDTO>> GetAllTariefCategoriesAsync();
        
        // Haal tariefcategorie op via ID
        Task<TariefCategorieDTO?> GetTariefCategorieByIdAsync(int categorieId);

        // ==== PLATFORMEN ====
        // Haal alle platformen
        Task<List<PlatformDTO>> GetAllPlatformsAsync();
        
        // Haal platform op via ID
        Task<PlatformDTO?> GetPlatformByIdAsync(int platformId);


        // ==== ACCOMMODATIE TYPES ====
        // Haal alle accommodatie types
        Task<List<AccommodatieTypeDTO>> GetAllAccommodatieTypesAsync();
        
        // Haal accommodatie type op via ID
        Task<AccommodatieTypeDTO?> GetAccommodatieTypeByIdAsync(int typeId);

        // ==== LOGBOEK (AUDIT TRAIL) ====
        // Maak een nieuwe log entry
        Task<int> CreateLogEntryAsync(LogboekDTO logEntry);
        
        // Haal recente log entries (default 50)
        Task<List<LogboekDTO>> GetRecentLogsAsync(int count = 50);
    }
}
