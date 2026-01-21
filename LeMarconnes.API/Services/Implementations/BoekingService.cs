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
    /// Implementatie van de volledige boekingsflow en reserveringsbeheer.
    /// </summary>
    public class BoekingService : IBoekingService
    {
        // ==== Properties ====
        private readonly IGiteRepository _repository;
        private readonly IBeschikbaarheidService _beschikbaarheid;
        private readonly IPrijsberekeningService _prijsberekening;

        // ==== Constructor ====
        public BoekingService(
            IGiteRepository repository,
            IBeschikbaarheidService beschikbaarheid,
            IPrijsberekeningService prijsberekening)
        {
            _repository = repository;
            _beschikbaarheid = beschikbaarheid;
            _prijsberekening = prijsberekening;
        }

        // ==== Public Methods ====

        public async Task<BoekingResponseDTO> MaakBoekingAsync(BoekingRequestDTO request)
        {
            // ==== Input Validatie ====
            if (request.EindDatum <= request.StartDatum)
                return BoekingResponseDTO.Failure("Einddatum moet na startdatum liggen.");

            // ==== Stap 1: Beschikbaarheidscheck ====
            var isBeschikbaar = await _beschikbaarheid.IsEenheidBeschikbaarAsync(
                request.EenheidID,
                request.StartDatum,
                request.EindDatum);

            if (!isBeschikbaar)
                return BoekingResponseDTO.Failure("Deze eenheid is niet beschikbaar in de gekozen periode.");

            // ==== Stap 2: Eenheid valideren ====
            var eenheid = await _repository.GetUnitByIdAsync(request.EenheidID);
            if (eenheid == null)
                return BoekingResponseDTO.Failure($"Eenheid met ID {request.EenheidID} niet gevonden.");

            // ==== Stap 3: Gast zoeken of aanmaken ====
            int gastId;
            var bestaandeGast = await _repository.GetGastByEmailAsync(request.GastEmail);

            if (bestaandeGast != null)
            {
                gastId = bestaandeGast.GastID;
            }
            else
            {
                var nieuweGast = new GastDTO
                {
                    Naam = request.GastNaam,
                    Email = request.GastEmail,
                    Tel = request.GastTel,
                    Straat = request.GastStraat,
                    Huisnr = request.GastHuisnr,
                    Postcode = request.GastPostcode,
                    Plaats = request.GastPlaats,
                    Land = request.GastLand
                };
                gastId = await _repository.CreateGastAsync(nieuweGast);
            }

            // ==== Stap 4: Prijs berekenen ====
            decimal totaalPrijs;
            try
            {
                totaalPrijs = await _prijsberekening.BerekenTotaalPrijsAsync(
                    request.EenheidID,
                    request.PlatformID,
                    request.StartDatum,
                    request.EindDatum,
                    request.AantalPersonen);
            }
            catch (Exception ex)
            {
                return BoekingResponseDTO.Failure($"Prijsberekening mislukt: {ex.Message}");
            }

            // ==== Stap 5: Reservering aanmaken ====
            var reservering = new ReserveringDTO
            {
                GastID = gastId,
                EenheidID = request.EenheidID,
                PlatformID = request.PlatformID,
                Startdatum = request.StartDatum,
                Einddatum = request.EindDatum,
                Status = "Gereserveerd"
            };
            int reserveringId = await _repository.CreateReservationAsync(reservering);

            // ==== Stap 6: Reservering detail aanmaken ====
            var tarief = await _prijsberekening.GetGeldigTariefAsync(
                eenheid.TypeID,
                request.PlatformID,
                request.StartDatum);

            if (tarief != null)
            {
                int aantalNachten = (request.EindDatum - request.StartDatum).Days;
                var detail = new ReserveringDetailDTO
                {
                    ReserveringID = reserveringId,
                    CategorieID = tarief.CategorieID,
                    Aantal = aantalNachten,
                    PrijsOpMoment = tarief.Prijs
                };
                await _repository.CreateReservationDetailAsync(detail);
            }

            // ==== Stap 7: Audit log ====
            await _repository.CreateLogEntryAsync(
                new LogboekDTO("RESERVERING_AANGEMAAKT", "RESERVERING", reserveringId));

            return BoekingResponseDTO.Success(
                reserveringId,
                eenheid.Naam,
                request.StartDatum,
                request.EindDatum,
                totaalPrijs);
        }

        public async Task<BoekingResponseDTO> WijzigReserveringAsync(
            int reserveringId,
            DateTime? nieuwStartDatum = null,
            DateTime? nieuwEindDatum = null,
            int? nieuweEenheidId = null,
            int? nieuwAantalPersonen = null)
        {
            // ==== Haal bestaande reservering op ====
            var bestaandeReservering = await _repository.GetReserveringByIdAsync(reserveringId);
            if (bestaandeReservering == null)
                return BoekingResponseDTO.Failure($"Reservering met ID {reserveringId} niet gevonden.");

            if (bestaandeReservering.Status == "Geannuleerd")
                return BoekingResponseDTO.Failure("Kan een geannuleerde reservering niet wijzigen.");

            // ==== Bepaal nieuwe waarden ====
            var startDatum = nieuwStartDatum ?? bestaandeReservering.Startdatum;
            var eindDatum = nieuwEindDatum ?? bestaandeReservering.Einddatum;
            var eenheidId = nieuweEenheidId ?? bestaandeReservering.EenheidID;

            if (eindDatum <= startDatum)
                return BoekingResponseDTO.Failure("Einddatum moet na startdatum liggen.");

            // ==== Check beschikbaarheid (skip huidige reservering) ====
            var isBeschikbaar = await _beschikbaarheid.IsEenheidBeschikbaarAsync(
                eenheidId,
                startDatum,
                eindDatum);

            if (!isBeschikbaar)
                return BoekingResponseDTO.Failure("De eenheid is niet beschikbaar voor de nieuwe periode.");

            // ==== Haal eenheid op ====
            var eenheid = await _repository.GetUnitByIdAsync(eenheidId);
            if (eenheid == null)
                return BoekingResponseDTO.Failure($"Eenheid met ID {eenheidId} niet gevonden.");

            // ==== Bereken nieuwe prijs ====
            var aantalPersonen = nieuwAantalPersonen ?? 1;
            decimal totaalPrijs;

            try
            {
                totaalPrijs = await _prijsberekening.BerekenTotaalPrijsAsync(
                    eenheidId,
                    bestaandeReservering.PlatformID,
                    startDatum,
                    eindDatum,
                    aantalPersonen);
            }
            catch (Exception ex)
            {
                return BoekingResponseDTO.Failure($"Prijsberekening mislukt: {ex.Message}");
            }

            // ==== Update reservering ====
            var gewijzigdeReservering = new ReserveringDTO
            {
                ReserveringID = reserveringId,
                GastID = bestaandeReservering.GastID,
                EenheidID = eenheidId,
                PlatformID = bestaandeReservering.PlatformID,
                Startdatum = startDatum,
                Einddatum = eindDatum,
                Status = bestaandeReservering.Status
            };

            bool success = await _repository.UpdateReservationAsync(gewijzigdeReservering);
            if (!success)
                return BoekingResponseDTO.Failure("Kon reservering niet bijwerken.");

            // ==== Audit log ====
            await _repository.CreateLogEntryAsync(
                new LogboekDTO("RESERVERING_GEWIJZIGD", "RESERVERING", reserveringId));

            return BoekingResponseDTO.Success(
                reserveringId,
                eenheid.Naam,
                startDatum,
                eindDatum,
                totaalPrijs);
        }

        public async Task<bool> AnnuleerReserveringAsync(int reserveringId)
        {
            var reservering = await _repository.GetReserveringByIdAsync(reserveringId);
            if (reservering == null)
                return false;

            var success = await _repository.UpdateReservationStatusAsync(reserveringId, "Geannuleerd");

            if (success)
            {
                await _repository.CreateLogEntryAsync(
                    new LogboekDTO("RESERVERING_GEANNULEERD", "RESERVERING", reserveringId));
            }

            return success;
        }

        public async Task<bool> WijzigStatusAsync(int reserveringId, string nieuweStatus)
        {
            var reservering = await _repository.GetReserveringByIdAsync(reserveringId);
            if (reservering == null)
                return false;

            var success = await _repository.UpdateReservationStatusAsync(reserveringId, nieuweStatus);

            if (success)
            {
                await _repository.CreateLogEntryAsync(
                    new LogboekDTO($"RESERVERING_STATUS_GEWIJZIGD_{nieuweStatus.ToUpper()}", "RESERVERING", reserveringId));
            }

            return success;
        }
    }
}
