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
    /// Controller voor reserveringsbeheer: boeken, wijzigen, annuleren, status updates.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ReserveringenController : BaseSecureController
    {
        // ==== Properties ====
        private readonly IBoekingService _boekingService;

        // ==== Constructor ====
        public ReserveringenController(IGiteRepository repository, IBoekingService boekingService)
            : base(repository)
        {
            _boekingService = boekingService;
        }

        // ============================================================
        // ==== USER ENDPOINTS ====
        // Authenticatie vereist: gebruiker moet ingelogd zijn om te boeken
        // ============================================================

        /// <summary>
        /// Maak een nieuwe boeking (volledige flow: gast + reservering + prijs).
        /// Gebruiker moet ingelogd zijn. Bij eerste boeking wordt GastDTO aangemaakt met NAW gegevens.
        /// </summary>
        /// <param name="request">Boekingsaanvraag met NAW-gegevens en reserveringsdetails</param>
        /// <returns>Boekingsbevestiging met reserveringsnummer en totaalprijs</returns>
        [Authorize(Roles = "User,Admin")]
        [HttpPost("boeken")]
        public async Task<ActionResult<BoekingResponseDTO>> Boeken([FromBody] BoekingRequestDTO request)
        {
            var resultaat = await _boekingService.MaakBoekingAsync(request);

            if (!resultaat.Succes)
                return BadRequest(resultaat);

            return Ok(resultaat);
        }

        /// <summary>
        /// Haal details van een specifieke reservering op.
        /// User role: alleen eigen reserveringen, Admin: alle reserveringen.
        /// </summary>
        /// <param name="id">Reservering ID</param>
        [Authorize(Roles = "User,Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<ReserveringDTO>> GetById(int id)
        {
            var reservering = await _repository.GetReserveringByIdAsync(id);

            if (reservering == null)
                return NotFound($"Reservering met ID {id} niet gevonden.");

            // Security check via base controller
            if (!await IsOwnerOfReserveringAsync(id))
                return Forbid();

            return Ok(reservering);
        }

        /// <summary>
        /// Haal alle reserveringen van een gast op.
        /// User role: alleen eigen reserveringen, Admin: alle reserveringen.
        /// </summary>
        /// <param name="gastId">Gast ID</param>
        [Authorize(Roles = "User,Admin")]
        [HttpGet("gast/{gastId}")]
        public async Task<ActionResult<List<ReserveringDTO>>> GetByGast(int gastId)
        {
            // Security check via base controller
            if (!await CanAccessGastDataAsync(gastId))
                return Forbid();

            var reserveringen = await _repository.GetReservationsForGastAsync(gastId);
            return Ok(reserveringen);
        }

        /// <summary>
        /// Haal kostenopbouw (details) van een reservering op.
        /// User role: alleen eigen reserveringen, Admin: alle reserveringen.
        /// </summary>
        /// <param name="id">Reservering ID</param>
        [Authorize(Roles = "User,Admin")]
        [HttpGet("{id}/details")]
        public async Task<ActionResult<List<ReserveringDetailDTO>>> GetDetails(int id)
        {
            // Haal eerst de reservering op om de GastID te kunnen checken
            var reservering = await _repository.GetReserveringByIdAsync(id);
            if (reservering == null)
                return NotFound($"Reservering met ID {id} niet gevonden.");

            // Security check via base controller
            if (!await IsOwnerOfReserveringAsync(id))
                return Forbid();

            var details = await _repository.GetReservationDetailsAsync(id);
            return Ok(details);
        }

        /// <summary>
        /// Wijzig een bestaande reservering (datums, eenheid, aantal personen).
        /// User role: alleen eigen reserveringen, Admin: alle reserveringen.
        /// </summary>
        /// <param name="id">Reservering ID</param>
        /// <param name="request">Nieuwe gegevens (alle velden optioneel)</param>
        [Authorize(Roles = "User,Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] UpdateReserveringRequestDTO request)
        {
            // Security check via base controller
            var reservering = await _repository.GetReserveringByIdAsync(id);
            if (reservering == null)
                return NotFound($"Reservering met ID {id} niet gevonden.");

            if (!await IsOwnerOfReserveringAsync(id))
                return Forbid();

            var resultaat = await _boekingService.WijzigReserveringAsync(
                id,
                request.StartDatum,
                request.EindDatum,
                request.EenheidID,
                request.AantalPersonen);

            if (!resultaat.Succes)
                return BadRequest(resultaat);

            return NoContent();
        }

        /// <summary>
        /// Annuleer een reservering (soft delete - status wordt "Geannuleerd").
        /// User role: alleen eigen reserveringen, Admin: alle reserveringen.
        /// </summary>
        /// <param name="id">Reservering ID</param>
        [Authorize(Roles = "User,Admin")]
        [HttpPatch("{id}/annuleer")]
        public async Task<ActionResult> Annuleer(int id)
        {
            // Security check: haal eerst reservering op om ownership te checken
            var reservering = await _repository.GetReserveringByIdAsync(id);
            if (reservering == null)
                return NotFound($"Reservering met ID {id} niet gevonden.");

            // Security check via base controller
            if (!await IsOwnerOfReserveringAsync(id))
                return Forbid();

            var success = await _boekingService.AnnuleerReserveringAsync(id);

            if (!success)
                return NotFound($"Reservering met ID {id} niet gevonden.");

            return NoContent();
        }

        // ============================================================
        // ==== ADMIN ENDPOINTS ====
        // ============================================================

        /// <summary>
        /// Haal alle reserveringen op (admin only).
        /// </summary>
        /// <param name="status">Optioneel: filter op status</param>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<List<ReserveringDTO>>> GetAll([FromQuery] string? status = null)
        {
            var reserveringen = await _repository.GetAllReserveringenAsync();

            // Filter op status indien opgegeven
            if (!string.IsNullOrEmpty(status))
            {
                reserveringen = reserveringen
                    .Where(r => r.Status.Equals(status, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            return Ok(reserveringen);
        }

        /// <summary>
        /// Wijzig de status van een reservering (admin only).
        /// </summary>
        /// <param name="id">Reservering ID</param>
        /// <param name="request">Nieuwe status (Gereserveerd, Ingecheckt, Uitgecheckt, Geannuleerd)</param>
        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/status")]
        public async Task<ActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequestDTO request)
        {
            var success = await _boekingService.WijzigStatusAsync(id, request.Status);

            if (!success)
                return NotFound($"Reservering met ID {id} niet gevonden.");

            return NoContent();
        }

        /// <summary>
        /// Verwijder een reservering permanent (hard delete - admin only).
        /// </summary>
        /// <param name="id">Reservering ID</param>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var reservering = await _repository.GetReserveringByIdAsync(id);
            if (reservering == null)
                return NotFound($"Reservering met ID {id} niet gevonden.");

            var success = await _repository.DeleteReservationAsync(id);
            if (!success)
                return BadRequest("Kon reservering niet verwijderen.");

            await LogActionAsync("RESERVERING_VERWIJDERD", "RESERVERING", id);

            return NoContent();
        }
    }
}
