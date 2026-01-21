// ======== Imports ========
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LeMarconnes.Shared.DTOs;

// ======== Namespace ========
namespace LeMarconnes.API.DAL.Interfaces
{
    // Interface voor repository operations van de Gîte module.
    // Kort, menselijk beschreven – details staan in implementatie.
    public interface IGiteRepository
    {
        // ==== VERHUUR EENHEDEN ====
        Task<List<VerhuurEenheidDTO>> GetAllGiteUnitsAsync();
        Task<VerhuurEenheidDTO?> GetUnitByIdAsync(int eenheidId);
        Task<List<VerhuurEenheidDTO>> GetChildUnitsAsync(int parentId);
        Task<int> CreateUnitAsync(VerhuurEenheidDTO unit);
        Task<bool> UpdateUnitAsync(VerhuurEenheidDTO unit);
        Task<bool> DeleteUnitAsync(int eenheidId);

        // ==== RESERVERINGEN ====
        Task<List<ReserveringDTO>> GetAllReserveringenAsync();
        Task<ReserveringDTO?> GetReserveringByIdAsync(int reserveringId);
        Task<List<ReserveringDTO>> GetReservationsByDateRangeAsync(DateTime startDatum, DateTime eindDatum);
        Task<List<ReserveringDTO>> GetReservationsForUnitAsync(int eenheidId, DateTime startDatum, DateTime eindDatum);
        Task<List<ReserveringDTO>> GetReservationsForGastAsync(int gastId);
        Task<int> CreateReservationAsync(ReserveringDTO reservering);
        Task<bool> UpdateReservationAsync(ReserveringDTO reservering);
        Task<bool> UpdateReservationStatusAsync(int reserveringId, string status);
        Task<bool> DeleteReservationAsync(int reserveringId);
        Task<int> CreateReservationDetailAsync(ReserveringDetailDTO detail);
        Task<List<ReserveringDetailDTO>> GetReservationDetailsAsync(int reserveringId);

        // ==== GASTEN ====
        Task<List<GastDTO>> GetAllGastenAsync();
        Task<GastDTO?> GetGastByEmailAsync(string email);
        Task<GastDTO?> GetGastByIdAsync(int gastId);
        Task<int> CreateGastAsync(GastDTO gast);
        Task<bool> UpdateGastAsync(GastDTO gast);
        Task<bool> AnonimiseerGastAsync(int gastId);

        // ==== GEBRUIKERS ====       
        Task<List<GebruikerDTO>> GetAllGebruikersAsync();
        Task<GebruikerDTO?> GetGebruikerByIdAsync(int gebruikerId);
        Task<GebruikerDTO?> GetGebruikerByEmailAsync(string email);
        Task<GebruikerDTO?> GetGebruikerByEmailMetVolledeGastAsync(string email);
        Task<int> CreateGebruikerAsync(GebruikerDTO gebruiker);
        Task<bool> UpdateGebruikerAsync(GebruikerDTO gebruiker);
        Task<bool> DeleteGebruikerAsync(int gebruikerId);
        Task<bool> UpdateGastIBANAsync(int gastId, string iban);

        // ==== TARIEVEN ====
        Task<TariefDTO?> GetTariefAsync(int typeId, int platformId, DateTime datum);
        Task<List<TariefDTO>> GetAllTarievenAsync();
        Task<TariefDTO?> GetTariefByIdAsync(int tariefId);
        Task<int> CreateTariefAsync(TariefDTO tarief);
        Task<bool> UpdateTariefAsync(TariefDTO tarief);
        Task<bool> DeleteTariefAsync(int tariefId);

        // ==== TARIEF CATEGORIEËN ====
        Task<List<TariefCategorieDTO>> GetAllTariefCategoriesAsync();
        Task<TariefCategorieDTO?> GetTariefCategorieByIdAsync(int categorieId);

        // ==== PLATFORMEN ====
        Task<List<PlatformDTO>> GetAllPlatformsAsync();
        Task<PlatformDTO?> GetPlatformByIdAsync(int platformId);

        // ==== ACCOMMODATIE TYPES ====
        Task<List<AccommodatieTypeDTO>> GetAllAccommodatieTypesAsync();
        Task<AccommodatieTypeDTO?> GetAccommodatieTypeByIdAsync(int typeId);
        Task<int> CreateAccommodatieTypeAsync(AccommodatieTypeDTO type);
        Task<bool> UpdateAccommodatieTypeAsync(AccommodatieTypeDTO type);
        Task<bool> DeleteAccommodatieTypeAsync(int typeId);

        // ==== LOGBOEK (AUDIT TRAIL) ====
        Task<int> CreateLogEntryAsync(LogboekDTO logEntry);
        Task<List<LogboekDTO>> GetRecentLogsAsync(int count = 50);
        Task<List<LogboekDTO>> GetLogsByEntiteitAsync(string tabelNaam, int recordId);
    }
}
