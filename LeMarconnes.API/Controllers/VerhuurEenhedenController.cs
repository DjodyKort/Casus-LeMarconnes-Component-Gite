// ======== Imports ========
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LeMarconnes.API.DAL.Interfaces;
using LeMarconnes.Shared.DTOs;

// ======== Namespace ========
namespace LeMarconnes.API.Controllers
{
    /// <summary>
    /// Controller voor verhuur eenheden beheer.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class VerhuurEenhedenController : ControllerBase
    {
        // ==== Properties ====
        private readonly IGiteRepository _repository;

        // ==== Constructor ====
        public VerhuurEenhedenController(IGiteRepository repository)
        {
            _repository = repository;
        }

        // ============================================================
        // ==== PUBLIC ENDPOINTS ====
        // ============================================================

        /// <summary>
        /// Haal alle actieve verhuur eenheden op.
        /// </summary>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<VerhuurEenheidDTO>>> GetAll()
        {
            var units = await _repository.GetAllGiteUnitsAsync();
            return Ok(units);
        }

        /// <summary>
        /// Haal een specifieke verhuur eenheid op.
        /// </summary>
        /// <param name="id">Eenheid ID</param>
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<VerhuurEenheidDTO>> GetById(int id)
        {
            var unit = await _repository.GetUnitByIdAsync(id);

            if (unit == null)
                return NotFound($"Eenheid met ID {id} niet gevonden.");

            return Ok(unit);
        }

        // ============================================================
        // ==== ADMIN ENDPOINTS ====
        // ============================================================

        /// <summary>
        /// Voeg een nieuwe verhuur eenheid toe (admin only).
        /// </summary>
        /// <param name="unit">Eenheid gegevens</param>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<VerhuurEenheidDTO>> Create([FromBody] VerhuurEenheidDTO unit)
        {
            int nieuweId = await _repository.CreateUnitAsync(unit);
            unit.EenheidID = nieuweId;

            await _repository.CreateLogEntryAsync(
                new LogboekDTO("EENHEID_AANGEMAAKT", "VERHUUR_EENHEID", nieuweId));

            return CreatedAtAction(nameof(GetById), new { id = nieuweId }, unit);
        }

        /// <summary>
        /// Wijzig een bestaande verhuur eenheid (admin only).
        /// </summary>
        /// <param name="id">Eenheid ID</param>
        /// <param name="unit">Nieuwe gegevens</param>
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] VerhuurEenheidDTO unit)
        {
            if (id != unit.EenheidID)
                return BadRequest("ID in URL komt niet overeen met ID in body.");

            var bestaandeEenheid = await _repository.GetUnitByIdAsync(id);
            if (bestaandeEenheid == null)
                return NotFound($"Eenheid met ID {id} niet gevonden.");

            var success = await _repository.UpdateUnitAsync(unit);
            if (!success)
                return BadRequest("Kon eenheid niet bijwerken.");

            await _repository.CreateLogEntryAsync(
                new LogboekDTO("EENHEID_GEWIJZIGD", "VERHUUR_EENHEID", id));

            return NoContent();
        }

        /// <summary>
        /// Verwijder een verhuur eenheid (admin only).
        /// Controleer eerst of er geen actieve reserveringen zijn.
        /// </summary>
        /// <param name="id">Eenheid ID</param>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var eenheid = await _repository.GetUnitByIdAsync(id);
            if (eenheid == null)
                return NotFound($"Eenheid met ID {id} niet gevonden.");

            // Check voor actieve reserveringen
            var reserveringen = await _repository.GetReservationsForUnitAsync(
                id,
                DateTime.Today,
                DateTime.Today.AddYears(1));

            if (reserveringen.Any(r => r.Status != "Geannuleerd"))
                return BadRequest("Kan eenheid niet verwijderen: er zijn nog actieve reserveringen.");

            var success = await _repository.DeleteUnitAsync(id);
            if (!success)
                return BadRequest("Kon eenheid niet verwijderen.");

            await _repository.CreateLogEntryAsync(
                new LogboekDTO("EENHEID_VERWIJDERD", "VERHUUR_EENHEID", id));

            return NoContent();
        }
    }
}
