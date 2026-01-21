// ======== Imports ========
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LeMarconnes.API.DAL.Interfaces;
using LeMarconnes.API.Services.Interfaces;
using LeMarconnes.Shared.DTOs;

// ======== Namespace ========
namespace LeMarconnes.API.Services.Implementations
{
    /// <summary>
    /// Implementatie van beschikbaarheidschecks met Parent-Child blokkade logica.
    /// </summary>
    public class BeschikbaarheidService : IBeschikbaarheidService
    {
        // ==== Constants ====
        private const int GITE_PARENT_ID = 1;

        // ==== Properties ====
        private readonly IGiteRepository _repository;

        // ==== Constructor ====
        public BeschikbaarheidService(IGiteRepository repository)
        {
            _repository = repository;
        }

        // ==== Public Methods ====

        public async Task<List<VerhuurEenheidDTO>> CheckBeschikbaarheidAsync(
            DateTime startDatum,
            DateTime eindDatum,
            int? aantalPersonen = null)
        {
            // Haal alle eenheden en overlappende reserveringen op
            var units = await _repository.GetAllGiteUnitsAsync() ?? new List<VerhuurEenheidDTO>();
            var reserveringen = await _repository.GetReservationsByDateRangeAsync(startDatum, eindDatum) ?? new List<ReserveringDTO>();

            // Bepaal geblokkeerde eenheden
            var geblokkeerdIds = BepaalGeblokkeerdEenheden(units, reserveringen);

            // Zet beschikbaarheid flag
            foreach (var unit in units)
            {
                unit.IsBeschikbaar = !geblokkeerdIds.Contains(unit.EenheidID);

                // Filter op capaciteit indien opgegeven
                if (aantalPersonen.HasValue && unit.MaxCapaciteit < aantalPersonen.Value)
                {
                    unit.IsBeschikbaar = false;
                }
            }

            // Log de check
            await _repository.CreateLogEntryAsync(
                new LogboekDTO("BESCHIKBAARHEID_CHECK", "VERHUUR_EENHEID"));

            return units;
        }

        public async Task<bool> IsEenheidBeschikbaarAsync(
            int eenheidId,
            DateTime startDatum,
            DateTime eindDatum)
        {
            var beschikbareEenheden = await CheckBeschikbaarheidAsync(startDatum, eindDatum);
            var eenheid = beschikbareEenheden.FirstOrDefault(e => e.EenheidID == eenheidId);

            return eenheid?.IsBeschikbaar ?? false;
        }

        public HashSet<int> BepaalGeblokkeerdEenheden(
            List<VerhuurEenheidDTO> units,
            List<ReserveringDTO> reserveringen)
        {
            var geblokkeerd = new HashSet<int>();
            var geboekteIds = reserveringen
                .Where(r => r.Status != "Geannuleerd")
                .Select(r => r.EenheidID)
                .ToHashSet();

            // SCENARIO 1: Parent is geboekt -> alles geblokkeerd
            if (geboekteIds.Contains(GITE_PARENT_ID))
            {
                geblokkeerd.Add(GITE_PARENT_ID);
                if (units != null)
                {
                    foreach (var unit in units.Where(u => u.ParentEenheidID == GITE_PARENT_ID))
                    {
                        geblokkeerd.Add(unit.EenheidID);
                    }
                }
            }
            // SCENARIO 2: Children zijn geboekt -> Die children + parent geblokkeerd
            else
            {
                if (units != null)
                {
                    foreach (var unit in units.Where(u => u.ParentEenheidID == GITE_PARENT_ID && geboekteIds.Contains(u.EenheidID)))
                    {
                        geblokkeerd.Add(unit.EenheidID);
                        geblokkeerd.Add(GITE_PARENT_ID);
                    }
                }
            }

            return geblokkeerd;
        }
    }
}
