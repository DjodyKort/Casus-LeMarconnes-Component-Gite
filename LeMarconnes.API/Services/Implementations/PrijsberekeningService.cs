// ======== Imports ========
using System;
using System.Threading.Tasks;
using LeMarconnes.API.DAL.Interfaces;
using LeMarconnes.API.Services.Interfaces;
using LeMarconnes.Shared.DTOs;

// ======== Namespace ========
namespace LeMarconnes.API.Services.Implementations
{
    /// <summary>
    /// Implementatie van prijsberekeningen en tarieflogica.
    /// </summary>
    public class PrijsberekeningService : IPrijsberekeningService
    {
        // ==== Properties ====
        private readonly IGiteRepository _repository;

        // ==== Constructor ====
        public PrijsberekeningService(IGiteRepository repository)
        {
            _repository = repository;
        }

        // ==== Public Methods ====

        public async Task<decimal> BerekenTotaalPrijsAsync(
            int eenheidId,
            int platformId,
            DateTime startDatum,
            DateTime eindDatum,
            int aantalPersonen = 1)
        {
            // Haal eenheid op
            var eenheid = await _repository.GetUnitByIdAsync(eenheidId);
            if (eenheid == null)
                throw new ArgumentException($"Eenheid met ID {eenheidId} niet gevonden.");

            // Haal tarief op
            var tarief = await GetGeldigTariefAsync(eenheid.TypeID, platformId, startDatum);
            if (tarief == null)
                throw new InvalidOperationException($"Geen geldig tarief gevonden voor TypeID {eenheid.TypeID}, PlatformID {platformId}");

            // Bereken aantal nachten
            int aantalNachten = (eindDatum - startDatum).Days;
            if (aantalNachten <= 0)
                throw new ArgumentException("Einddatum moet na startdatum liggen.");

            // Bereken prijs op basis van type
            decimal totaalPrijs;
            if (eenheid.TypeID == 1) // Geheel
            {
                totaalPrijs = tarief.Prijs * aantalNachten;
            }
            else // Slaapplek
            {
                totaalPrijs = tarief.Prijs * aantalPersonen * aantalNachten;
            }

            // Voeg toeristenbelasting toe indien niet inclusief
            if (!tarief.TaxStatus && tarief.TaxTarief > 0)
            {
                var belasting = BerekenToeristenbelasting(aantalPersonen, aantalNachten, tarief.TaxTarief);
                totaalPrijs += belasting;
            }

            return totaalPrijs;
        }

        public async Task<TariefDTO?> GetGeldigTariefAsync(
            int typeId,
            int platformId,
            DateTime datum)
        {
            return await _repository.GetTariefAsync(typeId, platformId, datum);
        }

        public decimal BerekenToeristenbelasting(
            int aantalPersonen,
            int aantalNachten,
            decimal tariefPerPersoonPerNacht)
        {
            return aantalPersonen * aantalNachten * tariefPerPersoonPerNacht;
        }
    }
}
