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
    /// <summary>
    /// Controller voor alle Gîte-gerelateerde API endpoints.
    /// Bevat de kernlogica voor het hybride verhuurmodel (Parent-Child beschikbaarheid).
    /// 
    /// Dit is de Business Logic Layer (BLL) van de applicatie.
    /// Alle endpoints zijn RESTful en retourneren JSON.
    /// 
    /// PARENT-CHILD LOGICA:
    /// - EenheidID 1 = "Gîte Totaal" (Parent)
    /// - EenheidID 2-10 = Individuele slaapplekken (Children)
    /// - Als Parent geboekt -> Alle Children geblokkeerd
    /// - Als 1+ Child geboekt -> Parent geblokkeerd
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class GiteController : ControllerBase {
        // ==== Properties ====
        // Repository voor database operaties (via Dependency Injection)
        private readonly IGiteRepository _repository;
        private const int GITE_PARENT_ID = 1;

        // ==== Constructor ====
        /// <summary>
        /// Constructor met Dependency Injection voor de repository.
        /// De repository wordt automatisch geïnjecteerd door ASP.NET Core.
        /// </summary>
        /// <param name="repository">De IGiteRepository implementatie</param>
        public GiteController(IGiteRepository repository)
        {
            _repository = repository;
        }

        // ============================================================
        // ==== EENHEDEN ENDPOINTS ====
        // Endpoints voor het ophalen van verhuur eenheden
        // ============================================================

        /// <summary>
        /// Haalt alle Gîte eenheden op.
        /// </summary>
        /// <returns>Lijst van alle verhuur eenheden</returns>
        /// <remarks>GET: api/gite/eenheden</remarks>
        [HttpGet("eenheden")]
        public async Task<ActionResult<List<VerhuurEenheidDTO>>> GetAllUnits()
        {
            // ==== Start of Function ====
            var units = await _repository.GetAllGiteUnitsAsync();
            return Ok(units);
        }

        /// <summary>
        /// Haalt een specifieke eenheid op basis van ID.
        /// </summary>
        /// <param name="id">Het EenheidID</param>
        /// <returns>De eenheid of 404 als niet gevonden</returns>
        /// <remarks>GET: api/gite/eenheden/{id}</remarks>
        [HttpGet("eenheden/{id}")]
        public async Task<ActionResult<VerhuurEenheidDTO>> GetUnitById(int id)
        {
            // ==== Start of Function ====
            var unit = await _repository.GetUnitByIdAsync(id);
            
            // 404 Not Found als eenheid niet bestaat
            if (unit == null)
                return NotFound($"Eenheid met ID {id} niet gevonden.");
            
            return Ok(unit);
        }

        // ============================================================
        // ==== BESCHIKBAARHEID ENDPOINT ===
        // Dit is de KERNLOGICA van het hybride verhuurmodel!
        // ============================================================

        /// <summary>
        /// Controleert de beschikbaarheid van alle eenheden voor een periode.
        /// Implementeert de Parent-Child blokkade logica.
        /// </summary>
        /// <param name="startDatum">Begin van de periode</param>
        /// <param name="eindDatum">Einde van de periode</param>
        /// <returns>Lijst van eenheden met IsBeschikbaar property</returns>
        /// <remarks>GET: api/gite/beschikbaarheid?startDatum=2025-06-01&eindDatum=2025-06-08</remarks>
        [HttpGet("beschikbaarheid")]
        public async Task<ActionResult<List<VerhuurEenheidDTO>>> CheckBeschikbaarheid(
            [FromQuery] DateTime startDatum,
            [FromQuery] DateTime eindDatum)
        {
            // ==== Input Validatie ====
            if (eindDatum <= startDatum)
                return BadRequest("Einddatum moet na startdatum liggen.");

            // ==== Start of Function ====
            // Haal alle eenheden op
            var units = await _repository.GetAllGiteUnitsAsync();
            
            // Haal alle overlappende reserveringen op
            var reserveringen = await _repository.GetReservationsByDateRangeAsync(startDatum, eindDatum);
            
            // Bepaal welke eenheden geblokkeerd zijn (Parent-Child logica)
            var geblokkeerdIds = BepaalGeblokkeerdEenheden(units, reserveringen);

            // Zet IsBeschikbaar property voor elke eenheid
            foreach (var unit in units)
            {
                unit.IsBeschikbaar = !geblokkeerdIds.Contains(unit.EenheidID);
            }

            // Log de actie voor audit trail
            await _repository.CreateLogEntryAsync(new LogboekDTO("BESCHIKBAARHEID_CHECK", "VERHUUR_EENHEID"));
            
            return Ok(units);
        }

        /// <summary>
        /// Bepaalt welke eenheden geblokkeerd zijn op basis van bestaande reserveringen.
        /// Dit is de kernlogica van het hybride verhuurmodel.
        /// 
        /// REGELS:
        /// 1. Als de Parent (Gîte Totaal) geboekt is:
        ///    -> Parent + ALLE Children zijn geblokkeerd
        /// 2. Als een of meer Children geboekt zijn:
        ///    -> Die Children + de Parent zijn geblokkeerd
        ///    -> Andere Children blijven beschikbaar
        /// </summary>
        /// <param name="units">Alle verhuur eenheden</param>
        /// <param name="reserveringen">Overlappende reserveringen</param>
        /// <returns>HashSet met geblokkeerde EenheidIDs</returns>
        private HashSet<int> BepaalGeblokkeerdEenheden(List<VerhuurEenheidDTO> units, List<ReserveringDTO> reserveringen)
        {
            // ==== Declaring Variables ====
            var geblokkeerd = new HashSet<int>();
            var geboekteIds = reserveringen.Select(r => r.EenheidID).ToHashSet();

            // ==== Start of Function ====
            // SCENARIO 1: Parent is geboekt -> alles geblokkeerd
            if (geboekteIds.Contains(GITE_PARENT_ID))
            {
                // Parent zelf toevoegen
                geblokkeerd.Add(GITE_PARENT_ID);
                
                // Alle children toevoegen
                foreach (var unit in units.Where(u => u.ParentEenheidID == GITE_PARENT_ID))
                    geblokkeerd.Add(unit.EenheidID);
            }
            // SCENARIO 2: Een of meer children zijn geboekt
            else
            {
                foreach (var unit in units.Where(u => u.ParentEenheidID == GITE_PARENT_ID && geboekteIds.Contains(u.EenheidID)))
                {
                    // De geboekte child blokkeren
                    geblokkeerd.Add(unit.EenheidID);
                    
                    // De parent ook blokkeren (kan niet meer als geheel verhuurd worden)
                    geblokkeerd.Add(GITE_PARENT_ID);
                }
            }
            
            return geblokkeerd;
        }

        // ============================================================
        // ==== RESERVERINGEN ENDPOINTS ====
        // CRUD operaties voor reserveringen
        // ============================================================

        /// <summary>
        /// Haalt alle reserveringen op, gesorteerd op startdatum (nieuwste eerst).
        /// </summary>
        /// <returns>Lijst van alle reserveringen</returns>
        /// <remarks>GET: api/gite/reserveringen</remarks>
        [HttpGet("reserveringen")]
        public async Task<ActionResult<List<ReserveringDTO>>> GetAllReserveringen()
        {
            // ==== Start of Function ====
            var reserveringen = await _repository.GetAllReserveringenAsync();
            return Ok(reserveringen);
        }

        /// <summary>
        /// Haalt een specifieke reservering op basis van ID.
        /// </summary>
        /// <param name="id">Het ReserveringID</param>
        /// <returns>De reservering of 404 als niet gevonden</returns>
        /// <remarks>GET: api/gite/reserveringen/{id}</remarks>
        [HttpGet("reserveringen/{id}")]
        public async Task<ActionResult<ReserveringDTO>> GetReserveringById(int id)
        {
            // ==== Start of Function ====
            var reservering = await _repository.GetReserveringByIdAsync(id);
            
            if (reservering == null)
                return NotFound($"Reservering met ID {id} niet gevonden.");
            
            return Ok(reservering);
        }

        /// <summary>
        /// Haalt alle reserveringen op voor een specifieke gast.
        /// </summary>
        /// <param name="gastId">Het GastID</param>
        /// <returns>Lijst van reserveringen voor deze gast</returns>
        /// <remarks>GET: api/gite/reserveringen/gast/{gastId}</remarks>
        [HttpGet("reserveringen/gast/{gastId}")]
        public async Task<ActionResult<List<ReserveringDTO>>> GetReserveringenByGast(int gastId)
        {
            // ==== Start of Function ====
            var reserveringen = await _repository.GetReservationsForGastAsync(gastId);
            return Ok(reserveringen);
        }

        /// <summary>
        /// Haalt de detailregels (kostenposten) van een reservering op.
        /// </summary>
        /// <param name="id">Het ReserveringID</param>
        /// <returns>Lijst van detail regels</returns>
        /// <remarks>GET: api/gite/reserveringen/{id}/details</remarks>
        [HttpGet("reserveringen/{id}/details")]
        public async Task<ActionResult<List<ReserveringDetailDTO>>> GetReserveringDetails(int id)
        {
            // ==== Start of Function ====
            var details = await _repository.GetReservationDetailsAsync(id);
            return Ok(details);
        }

        /// <summary>
        /// Maakt een nieuwe boeking aan.
        /// Dit is een complexe operatie die:
        /// 1. Beschikbaarheid controleert
        /// 2. Gast zoekt of aanmaakt
        /// 3. Tarief ophaalt
        /// 4. Prijs berekent
        /// 5. Reservering + Detail aanmaakt
        /// </summary>
        /// <param name="request">De boekingsgegevens</param>
        /// <returns>BoekingResponseDTO met resultaat</returns>
        /// <remarks>POST: api/gite/boek</remarks>
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
                // Bestaande gast gebruiken
                gastId = bestaandeGast.GastID;
            }
            else
            {
                // Nieuwe gast aanmaken
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
            
            // TypeID 1 = Geheel (prijs per nacht)
            // TypeID 2 = Slaapplek (prijs per persoon per nacht)
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

            // ---- Return success response ----
            return Ok(BoekingResponseDTO.Success(reserveringId, eenheid.Naam, request.StartDatum, request.EindDatum, totaalPrijs));
        }

        /// <summary>
        /// Annuleert een reservering (soft delete - status wordt "Geannuleerd").
        /// </summary>
        /// <param name="id">Het ReserveringID</param>
        /// <returns>Bevestigingsbericht of foutmelding</returns>
        /// <remarks>PUT: api/gite/reserveringen/{id}/annuleer</remarks>
        [HttpPut("reserveringen/{id}/annuleer")]
        public async Task<ActionResult> AnnuleerReservering(int id)
        {
            // ==== Start of Function ====
            // Check of reservering bestaat
            var reservering = await _repository.GetReserveringByIdAsync(id);
            if (reservering == null)
                return NotFound($"Reservering met ID {id} niet gevonden.");

            // Update status naar "Geannuleerd"
            var success = await _repository.UpdateReservationStatusAsync(id, "Geannuleerd");
            if (!success)
                return BadRequest("Kon reservering niet annuleren.");

            // Audit log
            await _repository.CreateLogEntryAsync(new LogboekDTO("RESERVERING_GEANNULEERD", "RESERVERING", id));
            
            return Ok(new { Message = $"Reservering {id} is geannuleerd." });
        }

        /// <summary>
        /// Verwijdert een reservering permanent (hard delete).
        /// Let op: Dit verwijdert ook alle gekoppelde detail regels!
        /// </summary>
        /// <param name="id">Het ReserveringID</param>
        /// <returns>204 No Content of foutmelding</returns>
        /// <remarks>DELETE: api/gite/reserveringen/{id}</remarks>
        [HttpDelete("reserveringen/{id}")]
        public async Task<ActionResult> DeleteReservering(int id)
        {
            // ==== Start of Function ====
            // Check of reservering bestaat
            var reservering = await _repository.GetReserveringByIdAsync(id);
            if (reservering == null)
                return NotFound($"Reservering met ID {id} niet gevonden.");

            // Verwijder de reservering
            var success = await _repository.DeleteReservationAsync(id);
            if (!success)
                return BadRequest("Kon reservering niet verwijderen.");

            // Audit log
            await _repository.CreateLogEntryAsync(new LogboekDTO("RESERVERING_VERWIJDERD", "RESERVERING", id));
            
            return NoContent();  // 204 - standaard response voor DELETE
        }

        // ============================================================
        // ==== GASTEN ENDPOINTS ====
        // CRUD operaties voor gasten
        // ============================================================

        /// <summary>
        /// Haalt alle gasten op, of zoekt op email als query parameter meegegeven.
        /// </summary>
        /// <param name="email">Optioneel: email om op te zoeken</param>
        /// <returns>Lijst van gasten</returns>
        /// <remarks>GET: api/gite/gasten of GET: api/gite/gasten?email=test@example.com</remarks>
        [HttpGet("gasten")]
        public async Task<ActionResult<List<GastDTO>>> GetAllGasten([FromQuery] string? email = null)
        {
            // ==== Start of Function ====
            // Als email parameter meegegeven, zoek op email
            if (!string.IsNullOrEmpty(email))
            {
                var gast = await _repository.GetGastByEmailAsync(email);
                if (gast == null)
                    return NotFound($"Gast met email '{email}' niet gevonden.");
                return Ok(new List<GastDTO> { gast });
            }

            // Anders: alle gasten ophalen
            var gasten = await _repository.GetAllGastenAsync();
            return Ok(gasten);
        }

        /// <summary>
        /// Haalt een specifieke gast op basis van ID.
        /// </summary>
        /// <param name="id">Het GastID</param>
        /// <returns>De gast of 404 als niet gevonden</returns>
        /// <remarks>GET: api/gite/gasten/{id}</remarks>
        [HttpGet("gasten/{id}")]
        public async Task<ActionResult<GastDTO>> GetGastById(int id)
        {
            // ==== Start of Function ====
            var gast = await _repository.GetGastByIdAsync(id);
            
            if (gast == null)
                return NotFound($"Gast met ID {id} niet gevonden.");
            
            return Ok(gast);
        }

        /// <summary>
        /// Maakt een nieuwe gast aan.
        /// Email moet uniek zijn.
        /// </summary>
        /// <param name="gast">De gastgegevens</param>
        /// <returns>201 Created met de nieuwe gast</returns>
        /// <remarks>POST: api/gite/gasten</remarks>
        [HttpPost("gasten")]
        public async Task<ActionResult<GastDTO>> CreateGast([FromBody] GastDTO gast)
        {
            // ==== Start of Function ====
            // Check of email al bestaat
            var bestaandeGast = await _repository.GetGastByEmailAsync(gast.Email);
            if (bestaandeGast != null)
                return Conflict($"Gast met email '{gast.Email}' bestaat al.");

            // Maak nieuwe gast aan
            int nieuweId = await _repository.CreateGastAsync(gast);
            gast.GastID = nieuweId;

            // Audit log
            await _repository.CreateLogEntryAsync(new LogboekDTO("GAST_AANGEMAAKT", "GAST", nieuweId));
            
            // Return 201 Created met Location header
            return CreatedAtAction(nameof(GetGastById), new { id = nieuweId }, gast);
        }

        /// <summary>
        /// Wijzigt een bestaande gast.
        /// </summary>
        /// <param name="id">Het GastID uit de URL</param>
        /// <param name="gast">De nieuwe gastgegevens</param>
        /// <returns>204 No Content of foutmelding</returns>
        /// <remarks>PUT: api/gite/gasten/{id}</remarks>
        [HttpPut("gasten/{id}")]
        public async Task<ActionResult> UpdateGast(int id, [FromBody] GastDTO gast)
        {
            // ==== Start of Function ====
            // Validatie: ID in URL moet overeenkomen met ID in body
            if (id != gast.GastID)
                return BadRequest("ID in URL komt niet overeen met ID in body.");

            // Check of gast bestaat
            var bestaandeGast = await _repository.GetGastByIdAsync(id);
            if (bestaandeGast == null)
                return NotFound($"Gast met ID {id} niet gevonden.");

            // Update de gast
            var success = await _repository.UpdateGastAsync(gast);
            if (!success)
                return BadRequest("Kon gast niet bijwerken.");

            // Audit log
            await _repository.CreateLogEntryAsync(new LogboekDTO("GAST_GEWIJZIGD", "GAST", id));
            
            return NoContent();
        }

        // ============================================================
        // ==== GEBRUIKERS ENDPOINTS ====
        // Read-only endpoints voor gebruikers (eigenaren)
        // ============================================================

        /// <summary>
        /// Haalt alle gebruikers (eigenaren) op.
        /// </summary>
        /// <returns>Lijst van gebruikers</returns>
        /// <remarks>GET: api/gite/gebruikers</remarks>
        [HttpGet("gebruikers")]
        public async Task<ActionResult<List<GebruikerDTO>>> GetAllGebruikers()
        {
            // ==== Start of Function ====
            var gebruikers = await _repository.GetAllGebruikersAsync();
            return Ok(gebruikers);
        }

        /// <summary>
        /// Haalt een specifieke gebruiker op basis van ID.
        /// </summary>
        /// <param name="id">Het GebruikerID</param>
        /// <returns>De gebruiker of 404 als niet gevonden</returns>
        /// <remarks>GET: api/gite/gebruikers/{id}</remarks>
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

        /// <summary>
        /// Haalt alle platformen op.
        /// </summary>
        /// <returns>Lijst van platformen</returns>
        /// <remarks>GET: api/gite/platformen</remarks>
        [HttpGet("platformen")]
        public async Task<ActionResult<List<PlatformDTO>>> GetAllPlatforms()
        {
            // ==== Start of Function ====
            var platforms = await _repository.GetAllPlatformsAsync();
            return Ok(platforms);
        }

        /// <summary>
        /// Haalt een specifiek platform op basis van ID.
        /// </summary>
        /// <param name="id">Het PlatformID</param>
        /// <returns>Het platform of 404 als niet gevonden</returns>
        /// <remarks>GET: api/gite/platformen/{id}</remarks>
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

        /// <summary>
        /// Haalt alle tarieven op.
        /// </summary>
        /// <returns>Lijst van tarieven</returns>
        /// <remarks>GET: api/gite/tarieven</remarks>
        [HttpGet("tarieven")]
        public async Task<ActionResult<List<TariefDTO>>> GetAllTarieven()
        {
            // ==== Start of Function ====
            var tarieven = await _repository.GetAllTarievenAsync();
            return Ok(tarieven);
        }

        /// <summary>
        /// Haalt een specifiek tarief op voor een type/platform combinatie.
        /// </summary>
        /// <param name="typeId">Het AccommodatieTypeID</param>
        /// <param name="platformId">Het PlatformID</param>
        /// <param name="datum">Optioneel: datum voor geldigheidscheck (default: vandaag)</param>
        /// <returns>Het geldige tarief of 404 als niet gevonden</returns>
        /// <remarks>GET: api/gite/tarieven/{typeId}/{platformId}?datum=2025-06-01</remarks>
        [HttpGet("tarieven/{typeId}/{platformId}")]
        public async Task<ActionResult<TariefDTO>> GetTarief(int typeId, int platformId, [FromQuery] DateTime? datum)
        {
            // ==== Start of Function ====
            // Gebruik vandaag als geen datum meegegeven
            var checkDatum = datum ?? DateTime.Today;
            
            var tarief = await _repository.GetTariefAsync(typeId, platformId, checkDatum);
            
            if (tarief == null)
                return NotFound($"Geen tarief gevonden voor TypeID {typeId}, PlatformID {platformId}");
            
            return Ok(tarief);
        }

        // ============================================================
        // ==== TARIEF CATEGORIEËN ENDPOINTS ====
        // Read-only endpoints voor tarief categorieën (lookup tabel)
        // ============================================================

        /// <summary>
        /// Haalt alle tarief categorieën op.
        /// </summary>
        /// <returns>Lijst van categorieën</returns>
        /// <remarks>GET: api/gite/tariefcategorieen</remarks>
        [HttpGet("tariefcategorieen")]
        public async Task<ActionResult<List<TariefCategorieDTO>>> GetAllTariefCategorieen()
        {
            // ==== Start of Function ====
            var categorieen = await _repository.GetAllTariefCategoriesAsync();
            return Ok(categorieen);
        }

        // ============================================================
        // ==== ACCOMMODATIE TYPES ENDPOINTS ====
        // Read-only endpoints voor accommodatie types (lookup tabel)
        // ============================================================

        /// <summary>
        /// Haalt alle accommodatie types op.
        /// </summary>
        /// <returns>Lijst van types</returns>
        /// <remarks>GET: api/gite/accommodatietypes</remarks>
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

        /// <summary>
        /// Haalt de meest recente log entries op.
        /// </summary>
        /// <param name="count">Aantal entries om op te halen (default: 50)</param>
        /// <returns>Lijst van log entries, nieuwste eerst</returns>
        /// <remarks>GET: api/gite/logs?count=50</remarks>
        [HttpGet("logs")]
        public async Task<ActionResult<List<LogboekDTO>>> GetRecentLogs([FromQuery] int count = 50)
        {
            // ==== Start of Function ====
            var logs = await _repository.GetRecentLogsAsync(count);
            return Ok(logs);
        }
    }
}
