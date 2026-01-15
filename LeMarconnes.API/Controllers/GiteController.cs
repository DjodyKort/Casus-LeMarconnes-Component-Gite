// ======== Imports ========
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LeMarconnes.API.DAL.Interfaces;
using LeMarconnes.Shared.DTOs;

// ======== Namespace ========
namespace LeMarconnes.API.Controllers
{
    // Controller voor Gîte API endpoints. Bevat business logic voor Parent-Child beschikbaarheid.
    [ApiController]
    [Route("api/[controller]")]
    public class GiteController : ControllerBase {
        // ==== Properties ====
        private readonly IGiteRepository _repository;
        private const int GITE_PARENT_ID = 1;

        // ==== Constructor ====
        // Repository wordt geïnjecteerd door ASP.NET Core DI
        public GiteController(IGiteRepository repository)
        {
            _repository = repository;
        }

        // ==== EENHEDEN ENDPOINTS ====
        // Endpoints voor het ophalen van verhuur eenheden

        // GET: api/gite/eenheden
        [HttpGet("eenheden")]
        public async Task<ActionResult<List<VerhuurEenheidDTO>>> GetAllUnits()
        {
            // ==== Start of Function ====
            var units = await _repository.GetAllGiteUnitsAsync();
            return Ok(units);
        }

        // GET: api/gite/eenheden/{id}
        [HttpGet("eenheden/{id}")]
        public async Task<ActionResult<VerhuurEenheidDTO>> GetUnitById(int id)
        {
            // ==== Start of Function ====
            var unit = await _repository.GetUnitByIdAsync(id);
            
            if (unit == null)
                return NotFound($"Eenheid met ID {id} niet gevonden.");
            
            return Ok(unit);
        }

        // ==== BESCHIKBAARHEID ENDPOINT ===
        // Dit is de KERNLOGICA van het hybride verhuurmodel!

        // GET: api/gite/beschikbaarheid?startDatum=2025-06-01&eindDatum=2025-06-08
        // Controleert beschikbaarheid en implementeert Parent-Child blokkade logica
        [HttpGet("beschikbaarheid")]
        public async Task<ActionResult<List<VerhuurEenheidDTO>>> CheckBeschikbaarheid(
            [FromQuery] DateTime startDatum,
            [FromQuery] DateTime eindDatum)
        {
            // ==== Input Validatie ====
            if (eindDatum <= startDatum)
                return BadRequest("Einddatum moet na startdatum liggen.");

            // ==== Start of Function ====
            var units = await _repository.GetAllGiteUnitsAsync();
            var reserveringen = await _repository.GetReservationsByDateRangeAsync(startDatum, eindDatum);
            var geblokkeerdIds = BepaalGeblokkeerdEenheden(units, reserveringen);

            foreach (var unit in units)
            {
                unit.IsBeschikbaar = !geblokkeerdIds.Contains(unit.EenheidID);
            }

            await _repository.CreateLogEntryAsync(new LogboekDTO("BESCHIKBAARHEID_CHECK", "VERHUUR_EENHEID"));
            
            return Ok(units);
        }

        // Bepaalt geblokkeerde eenheden op basis van Parent-Child logica:
        // - Als Parent geboekt -> Parent + alle Children geblokkeerd
        // - Als Child(ren) geboekt -> Die Children + Parent geblokkeerd
        private HashSet<int> BepaalGeblokkeerdEenheden(List<VerhuurEenheidDTO> units, List<ReserveringDTO> reserveringen)
        {
            // ==== Declaring Variables ====
            var geblokkeerd = new HashSet<int>();
            var geboekteIds = reserveringen.Select(r => r.EenheidID).ToHashSet();

            // ==== Start of Function ====
            // SCENARIO 1: Parent is geboekt -> alles geblokkeerd
            if (geboekteIds.Contains(GITE_PARENT_ID))
            {
                geblokkeerd.Add(GITE_PARENT_ID);
                foreach (var unit in units.Where(u => u.ParentEenheidID == GITE_PARENT_ID))
                    geblokkeerd.Add(unit.EenheidID);
            }
            // SCENARIO 2: Children zijn geboekt
            else
            {
                foreach (var unit in units.Where(u => u.ParentEenheidID == GITE_PARENT_ID && geboekteIds.Contains(u.EenheidID)))
                {
                    geblokkeerd.Add(unit.EenheidID);
                    geblokkeerd.Add(GITE_PARENT_ID);
                }
            }
            
            return geblokkeerd;
        }

        // ==== RESERVERINGEN ENDPOINTS ====
        // CRUD operaties voor reserveringen

        // GET: api/gite/reserveringen
        [HttpGet("reserveringen")]
        public async Task<ActionResult<List<ReserveringDTO>>> GetAllReserveringen()
        {
            // ==== Start of Function ====
            var reserveringen = await _repository.GetAllReserveringenAsync();
            return Ok(reserveringen);
        }

        // GET: api/gite/reserveringen/{id}
        [HttpGet("reserveringen/{id}")]
        public async Task<ActionResult<ReserveringDTO>> GetReserveringById(int id)
        {
            // ==== Start of Function ====
            var reservering = await _repository.GetReserveringByIdAsync(id);
            
            if (reservering == null)
                return NotFound($"Reservering met ID {id} niet gevonden.");
            
            return Ok(reservering);
        }

        // GET: api/gite/reserveringen/gast/{gastId}
        [HttpGet("reserveringen/gast/{gastId}")]
        public async Task<ActionResult<List<ReserveringDTO>>> GetReserveringenByGast(int gastId)
        {
            // ==== Start of Function ====
            var reserveringen = await _repository.GetReservationsForGastAsync(gastId);
            return Ok(reserveringen);
        }

        // GET: api/gite/reserveringen/{id}/details
        [HttpGet("reserveringen/{id}/details")]
        public async Task<ActionResult<List<ReserveringDetailDTO>>> GetReserveringDetails(int id)
        {
            // ==== Start of Function ====
            var details = await _repository.GetReservationDetailsAsync(id);
            return Ok(details);
        }

        // POST: api/gite/boek
        // Volledige boekingsflow: beschikbaarheid check, gast aanmaken, tarief ophalen, reservering aanmaken
        [HttpPost("boek")]
        public async Task<ActionResult<BoekingResponseDTO>> MaakBoeking([FromBody] BoekingRequestDTO request)
        {
            // ==== Declaring Variables ====
            int gastId, reserveringId, aantalNachten;
            decimal totaalPrijs;

            // ==== Input Validatie ====
            if (request.EindDatum <= request.StartDatum)
                return BadRequest(BoekingResponseDTO.Failure("Einddatum moet na startdatum liggen."));

            // ==== Start of Function ====
            
            // ---- Stap 1: Beschikbaarheidscheck ----
            var units = await _repository.GetAllGiteUnitsAsync();
            var reserveringen = await _repository.GetReservationsByDateRangeAsync(request.StartDatum, request.EindDatum);
            var geblokkeerdIds = BepaalGeblokkeerdEenheden(units, reserveringen);

            if (geblokkeerdIds.Contains(request.EenheidID))
                return Conflict(BoekingResponseDTO.Failure("Deze eenheid is niet beschikbaar in de gekozen periode."));

            // ---- Stap 2: Eenheid valideren ----
            var eenheid = await _repository.GetUnitByIdAsync(request.EenheidID);
            if (eenheid == null)
                return NotFound(BoekingResponseDTO.Failure($"Eenheid met ID {request.EenheidID} niet gevonden."));

            // ---- Stap 3: Gast zoeken of aanmaken ----
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

            // ---- Stap 4: Tarief ophalen ----
            var tarief = await _repository.GetTariefAsync(eenheid.TypeID, request.PlatformID, request.StartDatum);
            if (tarief == null)
                return BadRequest(BoekingResponseDTO.Failure("Geen geldig tarief gevonden."));

            // ---- Stap 5: Prijs berekenen ----
            aantalNachten = (request.EindDatum - request.StartDatum).Days;
            totaalPrijs = eenheid.TypeID == 1
                ? tarief.Prijs * aantalNachten
                : tarief.Prijs * request.AantalPersonen * aantalNachten;

            // ---- Stap 6: Reservering aanmaken ----
            var reservering = new ReserveringDTO
            {
                GastID = gastId,
                EenheidID = request.EenheidID,
                PlatformID = request.PlatformID,
                Startdatum = request.StartDatum,
                Einddatum = request.EindDatum,
                Status = "Gereserveerd"
            };
            reserveringId = await _repository.CreateReservationAsync(reservering);

            // ---- Stap 7: Reservering detail aanmaken ----
            var detail = new ReserveringDetailDTO
            {
                ReserveringID = reserveringId,
                CategorieID = tarief.CategorieID,
                Aantal = aantalNachten,
                PrijsOpMoment = tarief.Prijs
            };
            await _repository.CreateReservationDetailAsync(detail);

            // ---- Stap 8: Audit log ----
            await _repository.CreateLogEntryAsync(new LogboekDTO("RESERVERING_AANGEMAAKT", "RESERVERING", reserveringId));

            return Ok(BoekingResponseDTO.Success(reserveringId, eenheid.Naam, request.StartDatum, request.EindDatum, totaalPrijs));
        }

        // PUT: api/gite/reserveringen/{id}/annuleer
        // Soft delete: status wordt "Geannuleerd"
        [HttpPut("reserveringen/{id}/annuleer")]
        public async Task<ActionResult> AnnuleerReservering(int id)
        {
            // ==== Start of Function ====
            var reservering = await _repository.GetReserveringByIdAsync(id);
            if (reservering == null)
                return NotFound($"Reservering met ID {id} niet gevonden.");

            var success = await _repository.UpdateReservationStatusAsync(id, "Geannuleerd");
            if (!success)
                return BadRequest("Kon reservering niet annuleren.");

            await _repository.CreateLogEntryAsync(new LogboekDTO("RESERVERING_GEANNULEERD", "RESERVERING", id));
            
            return Ok(new { Message = $"Reservering {id} is geannuleerd." });
        }

        // DELETE: api/gite/reserveringen/{id}
        // Hard delete: verwijdert reservering + detailregels permanent
        [HttpDelete("reserveringen/{id}")]
        public async Task<ActionResult> DeleteReservering(int id)
        {
            // ==== Start of Function ====
            var reservering = await _repository.GetReserveringByIdAsync(id);
            if (reservering == null)
                return NotFound($"Reservering met ID {id} niet gevonden.");

            var success = await _repository.DeleteReservationAsync(id);
            if (!success)
                return BadRequest("Kon reservering niet verwijderen.");

            await _repository.CreateLogEntryAsync(new LogboekDTO("RESERVERING_VERWIJDERD", "RESERVERING", id));
            
            return NoContent();
        }

        // ==== GASTEN ENDPOINTS ====
        // CRUD operaties voor gasten

        // GET: api/gite/gasten of api/gite/gasten?email=test@example.com
        [HttpGet("gasten")]
        public async Task<ActionResult<List<GastDTO>>> GetAllGasten([FromQuery] string? email = null)
        {
            // ==== Start of Function ====
            if (!string.IsNullOrEmpty(email))
            {
                var gast = await _repository.GetGastByEmailAsync(email);
                if (gast == null)
                    return NotFound($"Gast met email '{email}' niet gevonden.");
                return Ok(new List<GastDTO> { gast });
            }

            var gasten = await _repository.GetAllGastenAsync();
            return Ok(gasten);
        }

        // GET: api/gite/gasten/{id}
        [HttpGet("gasten/{id}")]
        public async Task<ActionResult<GastDTO>> GetGastById(int id)
        {
            // ==== Start of Function ====
            var gast = await _repository.GetGastByIdAsync(id);
            
            if (gast == null)
                return NotFound($"Gast met ID {id} niet gevonden.");
            
            return Ok(gast);
        }

        // POST: api/gite/gasten
        // Email moet uniek zijn
        [HttpPost("gasten")]
        public async Task<ActionResult<GastDTO>> CreateGast([FromBody] GastDTO gast)
        {
            // ==== Start of Function ====
            var bestaandeGast = await _repository.GetGastByEmailAsync(gast.Email);
            if (bestaandeGast != null)
                return Conflict($"Gast met email '{gast.Email}' bestaat al.");

            int nieuweId = await _repository.CreateGastAsync(gast);
            gast.GastID = nieuweId;

            await _repository.CreateLogEntryAsync(new LogboekDTO("GAST_AANGEMAAKT", "GAST", nieuweId));
            
            return CreatedAtAction(nameof(GetGastById), new { id = nieuweId }, gast);
        }

        // PUT: api/gite/gasten/{id}
        [HttpPut("gasten/{id}")]
        public async Task<ActionResult> UpdateGast(int id, [FromBody] GastDTO gast)
        {
            // ==== Start of Function ====
            if (id != gast.GastID)
                return BadRequest("ID in URL komt niet overeen met ID in body.");

            var bestaandeGast = await _repository.GetGastByIdAsync(id);
            if (bestaandeGast == null)
                return NotFound($"Gast met ID {id} niet gevonden.");

            var success = await _repository.UpdateGastAsync(gast);
            if (!success)
                return BadRequest("Kon gast niet bijwerken.");

            await _repository.CreateLogEntryAsync(new LogboekDTO("GAST_GEWIJZIGD", "GAST", id));
            
            return NoContent();
        }

        // ============================================================
        // ==== GEBRUIKERS ENDPOINTS ====
        // Read-only endpoints voor gebruikers (eigenaren)
        // ============================================================

        // GET: api/gite/gebruikers
        [HttpGet("gebruikers")]
        public async Task<ActionResult<List<GebruikerDTO>>> GetAllGebruikers()
        {
            // ==== Start of Function ====
            var gebruikers = await _repository.GetAllGebruikersAsync();
            return Ok(gebruikers);
        }

        // GET: api/gite/gebruikers/{id}
        [HttpGet("gebruikers/{id}")]
        public async Task<ActionResult<GebruikerDTO>> GetGebruikerById(int id)
        {
            // ==== Start of Function ====
            var gebruiker = await _repository.GetGebruikerByIdAsync(id);
            
            if (gebruiker == null)
                return NotFound($"Gebruiker met ID {id} niet gevonden.");
            
            return Ok(gebruiker);
        }

        // ============================================================
        // ==== PLATFORMEN ENDPOINTS ====
        // Read-only endpoints voor platformen (Booking.com, Airbnb, etc.)
        // ============================================================

        // GET: api/gite/platformen
        [HttpGet("platformen")]
        public async Task<ActionResult<List<PlatformDTO>>> GetAllPlatforms()
        {
            // ==== Start of Function ====
            var platforms = await _repository.GetAllPlatformsAsync();
            return Ok(platforms);
        }

        // GET: api/gite/platformen/{id}
        [HttpGet("platformen/{id}")]
        public async Task<ActionResult<PlatformDTO>> GetPlatformById(int id)
        {
            // ==== Start of Function ====
            var platform = await _repository.GetPlatformByIdAsync(id);
            
            if (platform == null)
                return NotFound($"Platform met ID {id} niet gevonden.");
            
            return Ok(platform);
        }

        // ============================================================
        // ==== TARIEVEN ENDPOINTS ====
        // Read-only endpoints voor tarieven
        // ============================================================

        // GET: api/gite/tarieven
        [HttpGet("tarieven")]
        public async Task<ActionResult<List<TariefDTO>>> GetAllTarieven()
        {
            // ==== Start of Function ====
            var tarieven = await _repository.GetAllTarievenAsync();
            return Ok(tarieven);
        }

        // GET: api/gite/tarieven/{typeId}/{platformId}?datum=2025-06-01
        [HttpGet("tarieven/{typeId}/{platformId}")]
        public async Task<ActionResult<TariefDTO>> GetTarief(int typeId, int platformId, [FromQuery] DateTime? datum)
        {
            // ==== Start of Function ====
            var checkDatum = datum ?? DateTime.Today;
            var tarief = await _repository.GetTariefAsync(typeId, platformId, checkDatum);
            
            if (tarief == null)
                return NotFound($"Geen tarief gevonden voor TypeID {typeId}, PlatformID {platformId}");
            
            return Ok(tarief);
        }

        // ============================================================
        // ==== TARIEF CATEGORIEËN ENDPOINTS ====
        // Read-only endpoints voor tarief categorieën (lookup)
        // ============================================================

        // GET: api/gite/tariefcategorieen
        [HttpGet("tariefcategorieen")]
        public async Task<ActionResult<List<TariefCategorieDTO>>> GetAllTariefCategorieen()
        {
            // ==== Start of Function ====
            var categorieen = await _repository.GetAllTariefCategoriesAsync();
            return Ok(categorieen);
        }

        // ============================================================
        // ==== ACCOMMODATIE TYPES ENDPOINTS ====
        // Read-only endpoints voor accommodatie types (lookup)
        // ============================================================

        // GET: api/gite/accommodatietypes
        [HttpGet("accommodatietypes")]
        public async Task<ActionResult<List<AccommodatieTypeDTO>>> GetAllAccommodatieTypes()
        {
            // ==== Start of Function ====
            var types = await _repository.GetAllAccommodatieTypesAsync();
            return Ok(types);
        }

        // ============================================================
        // ==== LOGBOEK ENDPOINTS ====
        // Read-only endpoints voor de audit trail
        // ============================================================

        // GET: api/gite/logs?count=50
        [HttpGet("logs")]
        public async Task<ActionResult<List<LogboekDTO>>> GetRecentLogs([FromQuery] int count = 50)
        {
            // ==== Start of Function ====
            var logs = await _repository.GetRecentLogsAsync(count);
            return Ok(logs);
        }
    }
}
