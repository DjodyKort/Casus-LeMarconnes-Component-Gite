// ======== Imports ========
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LeMarconnes.API.DAL.Interfaces;
using LeMarconnes.API.Services.Interfaces;
using LeMarconnes.Shared.DTOs;

// ======== Namespace ========
namespace LeMarconnes.API.Controllers
{
    /// <summary>
    /// Controller voor tarieven beheer (prijzen, seizoenen).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TarievenController : ControllerBase
    {
        // ==== Properties ====
        private readonly IGiteRepository _repository;
        private readonly IPrijsberekeningService _prijsberekening;

        // ==== Constructor ====
        public TarievenController(IGiteRepository repository, IPrijsberekeningService prijsberekening)
        {
            _repository = repository;
            _prijsberekening = prijsberekening;
        }

        // ============================================================
        // ==== PUBLIC ENDPOINTS ====
        // ============================================================

        /// <summary>
        /// Haal alle tarieven op.
        /// </summary>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<TariefDTO>>> GetAll()
        {
            var tarieven = await _repository.GetAllTarievenAsync();
            return Ok(tarieven);
        }

        /// <summary>
        /// Haal geldig tarief op voor een specifieke combinatie.
        /// </summary>
        /// <param name="typeId">Accommodatie type ID</param>
        /// <param name="platformId">Platform ID</param>
        /// <param name="datum">Datum waarvoor tarief geldig moet zijn (default: vandaag)</param>
        [AllowAnonymous]
        [HttpGet("{typeId}/{platformId}")]
        public async Task<ActionResult<TariefDTO>> GetGeldigTarief(
            int typeId,
            int platformId,
            [FromQuery] DateTime? datum = null)
        {
            var checkDatum = datum ?? DateTime.Today;
            var tarief = await _prijsberekening.GetGeldigTariefAsync(typeId, platformId, checkDatum);

            if (tarief == null)
                return NotFound($"Geen geldig tarief gevonden voor TypeID {typeId}, PlatformID {platformId} op {checkDatum:yyyy-MM-dd}");

            return Ok(tarief);
        }

        /// <summary>
        /// Bereken totaalprijs voor een verblijf (preview zonder boeking).
        /// </summary>
        /// <param name="eenheidId">Verhuur eenheid ID</param>
        /// <param name="platformId">Platform ID</param>
        /// <param name="startDatum">Check-in datum</param>
        /// <param name="eindDatum">Check-out datum</param>
        /// <param name="aantalPersonen">Aantal personen (voor Type 2)</param>
        [AllowAnonymous]
        [HttpGet("berekenen")]
        public async Task<ActionResult<PrijsberekeningResponseDTO>> BerekenPrijs(
            [FromQuery] int eenheidId,
            [FromQuery] int platformId,
            [FromQuery] DateTime startDatum,
            [FromQuery] DateTime eindDatum,
            [FromQuery] int aantalPersonen = 1)
        {
            try
            {
                var totaalPrijs = await _prijsberekening.BerekenTotaalPrijsAsync(
                    eenheidId,
                    platformId,
                    startDatum,
                    eindDatum,
                    aantalPersonen);

                int aantalNachten = (eindDatum - startDatum).Days;

                return Ok(new PrijsberekeningResponseDTO
                {
                    TotaalPrijs = totaalPrijs,
                    AantalNachten = aantalNachten,
                    AantalPersonen = aantalPersonen,
                    PrijsPerNacht = totaalPrijs / aantalNachten
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        // ============================================================
        // ==== ADMIN ENDPOINTS ====
        // ============================================================

        /// <summary>
        /// Voeg een nieuw tarief toe (admin only).
        /// </summary>
        /// <param name="tarief">Tarief gegevens</param>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<TariefDTO>> Create([FromBody] TariefDTO tarief)
        {
            int nieuweId = await _repository.CreateTariefAsync(tarief);
            tarief.TariefID = nieuweId;

            await _repository.CreateLogEntryAsync(
                new LogboekDTO("TARIEF_AANGEMAAKT", "TARIEF", nieuweId));

            return CreatedAtAction(nameof(GetById), new { id = nieuweId }, tarief);
        }

        /// <summary>
        /// Haal een specifiek tarief op via ID.
        /// </summary>
        /// <param name="id">Tarief ID</param>
        [AllowAnonymous]
        [HttpGet("details/{id}")]
        public async Task<ActionResult<TariefDTO>> GetById(int id)
        {
            var tarief = await _repository.GetTariefByIdAsync(id);

            if (tarief == null)
                return NotFound($"Tarief met ID {id} niet gevonden.");

            return Ok(tarief);
        }

        /// <summary>
        /// Wijzig een bestaand tarief (admin only).
        /// </summary>
        /// <param name="id">Tarief ID</param>
        /// <param name="tarief">Nieuwe gegevens</param>
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] TariefDTO tarief)
        {
            if (id != tarief.TariefID)
                return BadRequest("ID in URL komt niet overeen met ID in body.");

            var bestaandTarief = await _repository.GetTariefByIdAsync(id);
            if (bestaandTarief == null)
                return NotFound($"Tarief met ID {id} niet gevonden.");

            var success = await _repository.UpdateTariefAsync(tarief);
            if (!success)
                return BadRequest("Kon tarief niet bijwerken.");

            await _repository.CreateLogEntryAsync(
                new LogboekDTO("TARIEF_GEWIJZIGD", "TARIEF", id));

            return NoContent();
        }

        /// <summary>
        /// Verwijder een tarief (admin only).
        /// Controleer eerst of er geen reserveringen met dit tarief zijn.
        /// </summary>
        /// <param name="id">Tarief ID</param>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var tarief = await _repository.GetTariefByIdAsync(id);
            if (tarief == null)
                return NotFound($"Tarief met ID {id} niet gevonden.");

            var success = await _repository.DeleteTariefAsync(id);
            if (!success)
                return BadRequest("Kon tarief niet verwijderen.");

            await _repository.CreateLogEntryAsync(
                new LogboekDTO("TARIEF_VERWIJDERD", "TARIEF", id));

            return NoContent();
        }
    }

    // ==== Response DTOs ====

    /// <summary>
    /// Response DTO voor prijsberekening.
    /// </summary>
    public class PrijsberekeningResponseDTO
    {
        public decimal TotaalPrijs { get; set; }
        public int AantalNachten { get; set; }
        public int AantalPersonen { get; set; }
        public decimal PrijsPerNacht { get; set; }
    }
}
