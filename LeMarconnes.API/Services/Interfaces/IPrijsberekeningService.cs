// ======== Imports ========
using System;
using System.Threading.Tasks;
using LeMarconnes.Shared.DTOs;

// ======== Namespace ========
namespace LeMarconnes.API.Services.Interfaces
{
    /// <summary>
    /// Service voor prijsberekeningen en tarieflogica.
    /// Ondersteunt twee prijsmodellen:
    /// - Type 1 (Geheel): Prijs per nacht
    /// - Type 2 (Slaapplek): Prijs per persoon per nacht
    /// </summary>
    public interface IPrijsberekeningService
    {
        /// <summary>
        /// Berekent de totaalprijs voor een verblijf inclusief alle kosten.
        /// </summary>
        /// <param name="eenheidId">ID van de verhuur eenheid</param>
        /// <param name="platformId">ID van het boekingsplatform</param>
        /// <param name="startDatum">Check-in datum</param>
        /// <param name="eindDatum">Check-out datum</param>
        /// <param name="aantalPersonen">Aantal personen (voor Type 2)</param>
        /// <returns>Totaalprijs voor het verblijf</returns>
        Task<decimal> BerekenTotaalPrijsAsync(
            int eenheidId,
            int platformId,
            DateTime startDatum,
            DateTime eindDatum,
            int aantalPersonen = 1);

        /// <summary>
        /// Haalt het geldige tarief op voor een specifieke combinatie.
        /// Geeft voorrang aan platform-specifieke tarieven.
        /// </summary>
        /// <param name="typeId">Accommodatie type ID</param>
        /// <param name="platformId">Platform ID</param>
        /// <param name="datum">Datum waarvoor tarief geldig moet zijn</param>
        /// <returns>Geldig tarief of null</returns>
        Task<TariefDTO?> GetGeldigTariefAsync(
            int typeId,
            int platformId,
            DateTime datum);

        /// <summary>
        /// Berekent toeristenbelasting voor een verblijf.
        /// </summary>
        /// <param name="aantalPersonen">Aantal personen</param>
        /// <param name="aantalNachten">Aantal nachten</param>
        /// <param name="tariefPerPersoonPerNacht">Belasting tarief</param>
        /// <returns>Totale toeristenbelasting</returns>
        decimal BerekenToeristenbelasting(
            int aantalPersonen,
            int aantalNachten,
            decimal tariefPerPersoonPerNacht);
    }
}
