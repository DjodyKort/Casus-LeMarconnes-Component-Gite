// ======== Imports ========
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LeMarconnes.Shared.DTOs;

// ======== Namespace ========
namespace LeMarconnes.API.Services.Interfaces
{
    /// <summary>
    /// Service voor beschikbaarheidschecks met Parent-Child blokkade logica.
    /// Implementeert het hybride verhuurmodel: Parent exclusief OF Children combineerbaar.
    /// </summary>
    public interface IBeschikbaarheidService
    {
        /// <summary>
        /// Controleert beschikbaarheid voor alle eenheden in een periode.
        /// Past Parent-Child blokkade logica toe.
        /// </summary>
        /// <param name="startDatum">Check-in datum</param>
        /// <param name="eindDatum">Check-out datum</param>
        /// <param name="aantalPersonen">Optioneel: filter op capaciteit</param>
        /// <returns>Lijst van eenheden met IsBeschikbaar flag</returns>
        Task<List<VerhuurEenheidDTO>> CheckBeschikbaarheidAsync(
            DateTime startDatum,
            DateTime eindDatum,
            int? aantalPersonen = null);

        /// <summary>
        /// Controleert of een specifieke eenheid beschikbaar is.
        /// </summary>
        /// <param name="eenheidId">ID van de eenheid</param>
        /// <param name="startDatum">Check-in datum</param>
        /// <param name="eindDatum">Check-out datum</param>
        /// <returns>True als beschikbaar</returns>
        Task<bool> IsEenheidBeschikbaarAsync(
            int eenheidId,
            DateTime startDatum,
            DateTime eindDatum);

        /// <summary>
        /// Bepaalt welke eenheden geblokkeerd zijn op basis van bestaande reserveringen.
        /// KERNLOGICA: 
        /// - Als Parent geboekt -> Parent + alle Children geblokkeerd
        /// - Als Child(ren) geboekt -> Die Children + Parent geblokkeerd
        /// </summary>
        /// <param name="units">Alle eenheden</param>
        /// <param name="reserveringen">Overlappende reserveringen</param>
        /// <returns>Set van geblokkeerde eenheid IDs</returns>
        HashSet<int> BepaalGeblokkeerdEenheden(
            List<VerhuurEenheidDTO> units,
            List<ReserveringDTO> reserveringen);
    }
}
