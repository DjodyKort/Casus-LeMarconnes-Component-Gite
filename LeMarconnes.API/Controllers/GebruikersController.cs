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
    /// Controller voor gebruikersbeheer: registratie, profiel, login.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class GebruikersController : ControllerBase
    {
        // ==== Properties ====
        private readonly IGiteRepository _repository;
        private readonly IWachtwoordService _wachtwoordService;

        // ==== Constructor ====
        public GebruikersController(IGiteRepository repository, IWachtwoordService wachtwoordService)
        {
            _repository = repository;
            _wachtwoordService = wachtwoordService;
        }

        // ============================================================
        // ==== PUBLIC ENDPOINTS ====
        // ============================================================

        /// <summary>
        /// Log in met email en wachtwoord.
        /// Verifieert credentials en retourneert gebruikersgegevens.
        /// </summary>
        /// <param name="request">Email en wachtwoord</param>
        /// <returns>Gebruiker object met profiel gegevens</returns>
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDTO>> Login([FromBody] LoginRequestDTO request)
        {
            // Valideer input
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Wachtwoord))
                return BadRequest("Email en wachtwoord zijn verplicht.");

            // Haal gebruiker op
            var gebruiker = await _repository.GetGebruikerByEmailAsync(request.Email);
            if (gebruiker == null)
                return Unauthorized("Ongeldige email of wachtwoord.");

            // Verificeer wachtwoord
            if (!_wachtwoordService.VerifyWachtwoord(request.Wachtwoord, gebruiker.WachtwoordHash))
                return Unauthorized("Ongeldige email of wachtwoord.");

            // Haal gekoppelde gast op indien aanwezig
            if (gebruiker.GastID.HasValue)
            {
                gebruiker.Gast = await _repository.GetGastByIdAsync(gebruiker.GastID.Value);
            }

            await _repository.CreateLogEntryAsync(
                new LogboekDTO("GEBRUIKER_INGELOGD", "GEBRUIKER", gebruiker.GebruikerID));

            // TODO: Genereer JWT token voor productie
            // Voor nu: return gebruiker + instructie om API key te gebruiken
            var response = new LoginResponseDTO
            {
                Succes = true,
                Bericht = "Login succesvol. Gebruik 'user-key-67890' als X-API-Key header voor authenticated requests.",
                Gebruiker = gebruiker
            };

            return Ok(response);
        }

        /// <summary>
        /// Registreer een nieuwe gebruiker (alleen email + wachtwoord).
        /// NAW gegevens worden later toegevoegd bij eerste reservering.
        /// </summary>
        /// <param name="request">Email en wachtwoord</param>
        /// <returns>Nieuw gebruiker object (zonder wachtwoord hash)</returns>
        [AllowAnonymous]
        [HttpPost("registreren")]
        public async Task<ActionResult<GebruikerDTO>> Registreren([FromBody] RegisterGebruikerRequestDTO request)
        {
            // Valideer input
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Wachtwoord))
                return BadRequest("Email en wachtwoord zijn verplicht.");

            // Check of email al bestaat
            var bestaandeGebruiker = await _repository.GetGebruikerByEmailAsync(request.Email);
            if (bestaandeGebruiker != null)
                return Conflict($"Gebruiker met email '{request.Email}' bestaat al.");

            // Maak gebruiker aan (GastID blijft NULL tot eerste reservering)
            var nieuweGebruiker = new GebruikerDTO
            {
                Email = request.Email,
                WachtwoordHash = _wachtwoordService.HashWachtwoord(request.Wachtwoord),
                Rol = "User",
                GastID = null // Wordt later gekoppeld bij eerste reservering
            };

            int nieuweId = await _repository.CreateGebruikerAsync(nieuweGebruiker);
            nieuweGebruiker.GebruikerID = nieuweId;

            await _repository.CreateLogEntryAsync(
                new LogboekDTO("GEBRUIKER_GEREGISTREERD", "GEBRUIKER", nieuweId));

            // Return zonder WachtwoordHash (JsonIgnore zorgt hiervoor)
            return CreatedAtAction(nameof(GetProfiel), new { }, nieuweGebruiker);
        }

        // ============================================================
        // ==== USER ENDPOINTS ====
        // ============================================================

        /// <summary>
        /// Haal eigen gebruikersprofiel op (inclusief gekoppelde gast gegevens indien aanwezig).
        /// </summary>
        /// <returns>Gebruiker met optioneel gekoppelde Gast (met volledige NAW gegevens)</returns>
        [Authorize(Roles = "User,Admin")]
        [HttpGet("profiel")]
        public async Task<ActionResult<GebruikerDTO>> GetProfiel()
        {
            // TODO: Haal gebruiker ID uit JWT token/claims wanneer JWT wordt geïmplementeerd
            // Voor nu: gebruik email uit context (via API key)
            var email = User.Identity?.Name ?? "unknown";

            // Gebruik de geoptimaliseerde methode die volledige Gast data ophaalt
            var gebruiker = await _repository.GetGebruikerByEmailMetVolledeGastAsync(email);
            if (gebruiker == null)
                return NotFound("Gebruiker niet gevonden.");

            return Ok(gebruiker);
        }

        /// <summary>
        /// Update eigen wachtwoord.
        /// </summary>
        /// <param name="request">Oud en nieuw wachtwoord</param>
        [Authorize(Roles = "User,Admin")]
        [HttpPatch("wachtwoord")]
        public async Task<ActionResult> UpdateWachtwoord([FromBody] UpdateWachtwoordRequestDTO request)
        {
            var email = User.Identity?.Name ?? "unknown";
            var gebruiker = await _repository.GetGebruikerByEmailAsync(email);

            if (gebruiker == null)
                return NotFound("Gebruiker niet gevonden.");

            // Verificeer oud wachtwoord
            if (!_wachtwoordService.VerifyWachtwoord(request.OudWachtwoord, gebruiker.WachtwoordHash))
                return BadRequest("Huidig wachtwoord is incorrect.");

            // Update wachtwoord
            gebruiker.WachtwoordHash = _wachtwoordService.HashWachtwoord(request.NieuwWachtwoord);
            var success = await _repository.UpdateGebruikerAsync(gebruiker);

            if (!success)
                return BadRequest("Kon wachtwoord niet bijwerken.");

            await _repository.CreateLogEntryAsync(
                new LogboekDTO("WACHTWOORD_GEWIJZIGD", "GEBRUIKER", gebruiker.GebruikerID));

            return NoContent();
        }

        // ============================================================
        // ==== ADMIN ENDPOINTS ====
        // ============================================================

        /// <summary>
        /// Haal alle gebruikers op (admin only).
        /// Wordt ook gebruikt door LookupsController.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<List<GebruikerDTO>>> GetAll()
        {
            var gebruikers = await _repository.GetAllGebruikersAsync();
            return Ok(gebruikers);
        }

        /// <summary>
        /// Haal specifieke gebruiker op via ID (admin only).
        /// </summary>
        /// <param name="id">Gebruiker ID</param>
        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<GebruikerDTO>> GetById(int id)
        {
            var gebruiker = await _repository.GetGebruikerByIdAsync(id);

            if (gebruiker == null)
                return NotFound($"Gebruiker met ID {id} niet gevonden.");

            // Haal gekoppelde gast op indien aanwezig
            if (gebruiker.GastID.HasValue)
            {
                gebruiker.Gast = await _repository.GetGastByIdAsync(gebruiker.GastID.Value);
            }

            return Ok(gebruiker);
        }

        /// <summary>
        /// Verwijder een gebruiker (admin only).
        /// Let op: dit verwijdert de login, maar behoudt Gast gegevens voor historie.
        /// </summary>
        /// <param name="id">Gebruiker ID</param>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var gebruiker = await _repository.GetGebruikerByIdAsync(id);
            if (gebruiker == null)
                return NotFound($"Gebruiker met ID {id} niet gevonden.");

            var success = await _repository.DeleteGebruikerAsync(id);
            if (!success)
                return BadRequest("Kon gebruiker niet verwijderen.");

            await _repository.CreateLogEntryAsync(
                new LogboekDTO("GEBRUIKER_VERWIJDERD", "GEBRUIKER", id));

            return NoContent();
        }
    }
}
