// ======== Imports ========
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LeMarconnes.API.DAL.Interfaces;
using LeMarconnes.Shared.DTOs;

// ======== Namespace ========
namespace LeMarconnes.API.Controllers
{
    /// <summary>
    /// Controller voor beheer van accommodatie types (Geheel, Slaapplek).
    /// GET endpoints zijn publiek beschikbaar voor dropdowns.
    /// POST/PUT/DELETE zijn admin-only endpoints.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AccommodatieTypesController : ControllerBase
    {
        // ==== Properties ====
        private readonly IGiteRepository _repository;

        // ==== Constructor ====
        public AccommodatieTypesController(IGiteRepository repository) {
            _repository = repository;
        }

        // ==== READ OPERATIONS (PUBLIC) ====
        /// <summary>
        /// Haal alle accommodatie types op (Geheel, Slaapplek).
        /// </summary>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<AccommodatieTypeDTO>>> GetAll() {
            var types = await _repository.GetAllAccommodatieTypesAsync();
            return Ok(types);
        }

        /// <summary>
        /// Haal een specifiek accommodatie type op.
        /// </summary>
        /// <param name="id">Type ID</param>
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<AccommodatieTypeDTO>> GetById(int id) {
            var type = await _repository.GetAccommodatieTypeByIdAsync(id);

            if (type == null)
                return NotFound($"Accommodatie type met ID {id} niet gevonden.");

            return Ok(type);
        }

        // ==== WRITE OPERATIONS (ADMIN ONLY) ====

        /// <summary>
        /// Voeg een nieuw accommodatie type toe (admin only).
        /// </summary>
        /// <param name="type">Type gegevens</param>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<AccommodatieTypeDTO>> Create([FromBody] AccommodatieTypeDTO type)
        {
            int nieuweId = await _repository.CreateAccommodatieTypeAsync(type);
            type.TypeID = nieuweId;

            await _repository.CreateLogEntryAsync(
                new LogboekDTO("ACCOMMODATIE_TYPE_AANGEMAAKT", "ACCOMMODATIE_TYPE", nieuweId));

            return CreatedAtAction(nameof(GetById), new { id = nieuweId }, type);
        }

        /// <summary>
        /// Wijzig een accommodatie type (admin only).
        /// </summary>
        /// <param name="id">Type ID</param>
        /// <param name="type">Nieuwe gegevens</param>
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] AccommodatieTypeDTO type)
        {
            if (id != type.TypeID)
                return BadRequest("ID in URL komt niet overeen met ID in body.");

            var bestaandType = await _repository.GetAccommodatieTypeByIdAsync(id);
            if (bestaandType == null)
                return NotFound($"Accommodatie type met ID {id} niet gevonden.");

            var success = await _repository.UpdateAccommodatieTypeAsync(type);
            if (!success)
                return BadRequest("Kon accommodatie type niet bijwerken.");

            await _repository.CreateLogEntryAsync(
                new LogboekDTO("ACCOMMODATIE_TYPE_GEWIJZIGD", "ACCOMMODATIE_TYPE", id));

            return NoContent();
        }

        /// <summary>
        /// Verwijder een accommodatie type (admin only).
        /// Controleer eerst of er geen eenheden van dit type zijn.
        /// </summary>
        /// <param name="id">Type ID</param>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var type = await _repository.GetAccommodatieTypeByIdAsync(id);
            if (type == null)
                return NotFound($"Accommodatie type met ID {id} niet gevonden.");

            // Check of er eenheden zijn met dit type
            var units = await _repository.GetAllGiteUnitsAsync();
            if (units != null && units.Any(u => u.TypeID == id))
                return BadRequest("Kan accommodatie type niet verwijderen: er zijn nog eenheden van dit type.");

            var success = await _repository.DeleteAccommodatieTypeAsync(id);
            if (!success)
                return BadRequest("Kon accommodatie type niet verwijderen.");

            await _repository.CreateLogEntryAsync(
                new LogboekDTO("ACCOMMODATIE_TYPE_VERWIJDERD", "ACCOMMODATIE_TYPE", id));

            return NoContent();
        }
    }
}
