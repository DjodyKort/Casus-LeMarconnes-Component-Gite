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
    /// Controller voor gastenbeheer (NAW gegevens).
    /// Gast wordt automatisch aangemaakt bij eerste reservering door een ingelogde gebruiker.
    /// IBAN wordt toegevoegd via betaalwebhook, niet handmatig.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class GastenController : BaseSecureController
    {
        // ==== Constructor ====
        public GastenController(IGiteRepository repository)
            : base(repository)
        {
        }

        // ============================================================
        // ==== PUBLIC/USER ENDPOINTS ====
        // ============================================================

        /// <summary>
        /// Haal een specifieke gast op via ID.
        /// User role: alleen eigen gast gegevens, Admin: alle gasten.
        /// </summary>
        /// <param name="id">Gast ID</param>
        [Authorize(Roles = "User,Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<GastDTO>> GetById(int id)
        {
            var gast = await _repository.GetGastByIdAsync(id);

            if (gast == null)
                return NotFound($"Gast met ID {id} niet gevonden.");

            // Security check via base controller
            if (!await CanAccessGastDataAsync(id))
                return Forbid();

            return Ok(gast);
        }

        /// <summary>
        /// Update gast gegevens (NAW wijzigen).
        /// User role: alleen eigen gast, Admin: alle gasten.
        /// </summary>
        /// <param name="id">Gast ID</param>
        /// <param name="gast">Nieuwe gegevens</param>
        [Authorize(Roles = "User,Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] GastDTO gast)
        {
            if (id != gast.GastID)
                return BadRequest("ID in URL komt niet overeen met ID in body.");

            var bestaandeGast = await _repository.GetGastByIdAsync(id);
            if (bestaandeGast == null)
                return NotFound($"Gast met ID {id} niet gevonden.");

            // Security check via base controller
            if (!await CanAccessGastDataAsync(id))
                return Forbid();

            var success = await _repository.UpdateGastAsync(gast);
            if (!success)
                return BadRequest("Kon gast niet bijwerken.");

            await LogActionAsync("GAST_GEWIJZIGD", "GAST", id);

            return NoContent();
        }

        // ============================================================
        // ==== ADMIN ENDPOINTS ====
        // ============================================================

        /// <summary>
        /// Update IBAN voor een gast (webhook endpoint voor betaalprovider).
        /// Dit endpoint wordt aangeroepen door Mollie/Stripe na een succesvolle betaling.
        /// BELANGRIJK: In productie moet signature validatie geïmplementeerd worden!
        /// </summary>
        /// <param name="request">GastID en IBAN</param>
        [Authorize(Roles = "Admin")] // Webhook moet admin key gebruiken voor beveiliging
        [HttpPost("webhook/iban")]
        public async Task<ActionResult> UpdateIBAN([FromBody] UpdateIBANRequestDTO request)
        {
            // TODO PRODUCTIE: Valideer webhook signature van betaalprovider
            // Voorbeeld voor Mollie:
            // string signature = Request.Headers["X-Mollie-Signature"];
            // string payload = await new StreamReader(Request.Body).ReadToEndAsync();
            // if (!VerifyMollieSignature(payload, signature, mollieSecret))
            //     return Unauthorized("Invalid signature");

            var gast = await _repository.GetGastByIdAsync(request.GastID);
            if (gast == null)
                return NotFound($"Gast met ID {request.GastID} niet gevonden.");

            // Update IBAN
            var success = await _repository.UpdateGastIBANAsync(request.GastID, request.IBAN);
            if (!success)
                return BadRequest("Kon IBAN niet bijwerken.");

            await LogActionAsync("IBAN_TOEGEVOEGD_VIA_WEBHOOK", "GAST", request.GastID);

            return Ok(new { message = "IBAN succesvol toegevoegd" });
        }

        /// <summary>
        /// Haal alle gasten op (admin only).
        /// </summary>
        /// <param name="zoek">Optioneel: zoek op email of naam</param>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<List<GastDTO>>> GetAll([FromQuery] string? zoek = null)
        {
            if (!string.IsNullOrEmpty(zoek))
            {
                // Zoek op email
                var gast = await _repository.GetGastByEmailAsync(zoek);
                if (gast == null)
                    return NotFound($"Gast met email '{zoek}' niet gevonden.");
                return Ok(new List<GastDTO> { gast });
            }

            var gasten = await _repository.GetAllGastenAsync();
            return Ok(gasten);
        }

        /// <summary>
        /// Maak een nieuwe gast aan (admin only).
        /// Normaal gesproken gebeurt dit automatisch bij boeken.
        /// IBAN wordt NIET handmatig ingevuld maar via betaalwebhook.
        /// </summary>
        /// <param name="gast">Gast gegevens (zonder IBAN)</param>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<GastDTO>> Create([FromBody] GastDTO gast)
        {
            var bestaandeGast = await _repository.GetGastByEmailAsync(gast.Email);
            if (bestaandeGast != null)
                return Conflict($"Gast met email '{gast.Email}' bestaat al.");

            int nieuweId = await _repository.CreateGastAsync(gast);
            gast.GastID = nieuweId;

            await LogActionAsync("GAST_AANGEMAAKT", "GAST", nieuweId);

            return CreatedAtAction(nameof(GetById), new { id = nieuweId }, gast);
        }

        /// <summary>
        /// Anonimiseer een gast (GDPR compliance - admin only).
        /// Verwijdert persoonlijke gegevens maar behoudt reservering historie.
        /// </summary>
        /// <param name="id">Gast ID</param>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Anonimiseer(int id)
        {
            var gast = await _repository.GetGastByIdAsync(id);
            if (gast == null)
                return NotFound($"Gast met ID {id} niet gevonden.");

            var success = await _repository.AnonimiseerGastAsync(id);
            if (!success)
                return BadRequest("Kon gast niet anonimiseren.");

            await LogActionAsync("GAST_GEANONIMISEERD_GDPR", "GAST", id);

            return NoContent();
        }
    }
}
