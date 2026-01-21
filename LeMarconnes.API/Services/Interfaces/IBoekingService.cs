// ======== Imports ========
using System;
using System.Threading.Tasks;
using LeMarconnes.Shared.DTOs;

// ======== Namespace ========
namespace LeMarconnes.API.Services.Interfaces
{
    /// <summary>
    /// Service voor de volledige boekingsflow en reserveringsbeheer.
    /// Coördineert tussen beschikbaarheid, prijsberekening, en database operaties.
    /// </summary>
    public interface IBoekingService
    {
        /// <summary>
        /// Volledige boekingsflow: beschikbaarheid check, gast aanmaken/ophalen, 
        /// tarief berekenen, reservering aanmaken met details.
        /// </summary>
        /// <param name="request">Boekingsaanvraag met gast- en reserveringsgegevens</param>
        /// <returns>Success of failure response met reserveringsnummer en totaalprijs</returns>
        Task<BoekingResponseDTO> MaakBoekingAsync(BoekingRequestDTO request);

        /// <summary>
        /// Update een bestaande reservering (datums, eenheid, aantal personen).
        /// Controleert opnieuw beschikbaarheid en herberekent prijs.
        /// </summary>
        /// <param name="reserveringId">ID van de reservering</param>
        /// <param name="nieuwStartDatum">Nieuwe check-in datum (optioneel)</param>
        /// <param name="nieuwEindDatum">Nieuwe check-out datum (optioneel)</param>
        /// <param name="nieuweEenheidId">Nieuwe eenheid (optioneel)</param>
        /// <param name="nieuwAantalPersonen">Nieuw aantal personen (optioneel)</param>
        /// <returns>Success of failure met updated gegevens</returns>
        Task<BoekingResponseDTO> WijzigReserveringAsync(
            int reserveringId,
            DateTime? nieuwStartDatum = null,
            DateTime? nieuwEindDatum = null,
            int? nieuweEenheidId = null,
            int? nieuwAantalPersonen = null);

        /// <summary>
        /// Annuleer een reservering (soft delete - status wijzigt naar "Geannuleerd").
        /// </summary>
        /// <param name="reserveringId">ID van de reservering</param>
        /// <returns>True als succesvol</returns>
        Task<bool> AnnuleerReserveringAsync(int reserveringId);

        /// <summary>
        /// Wijzig de status van een reservering (Ingecheckt, Uitgecheckt, etc).
        /// </summary>
        /// <param name="reserveringId">ID van de reservering</param>
        /// <param name="nieuweStatus">Nieuwe status</param>
        /// <returns>True als succesvol</returns>
        Task<bool> WijzigStatusAsync(int reserveringId, string nieuweStatus);
    }
}
