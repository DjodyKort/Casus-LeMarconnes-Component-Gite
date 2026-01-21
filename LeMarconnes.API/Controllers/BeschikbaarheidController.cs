// ======== Imports ========
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LeMarconnes.API.Services.Interfaces;
using LeMarconnes.Shared.DTOs;

// ======== Namespace ========
namespace LeMarconnes.API.Controllers
{
    /// <summary>
    /// Controller voor beschikbaarheidschecks met Parent-Child logica.
    /// </summary>
    [AllowAnonymous]
    [ApiController]
    [Route("api/[controller]")]
    public class BeschikbaarheidController : ControllerBase
    {
        // ==== Properties ====
        private readonly IBeschikbaarheidService _beschikbaarheidService;

        // ==== Constructor ====
        public BeschikbaarheidController(IBeschikbaarheidService beschikbaarheidService)
        {
            _beschikbaarheidService = beschikbaarheidService;
        }

        // ============================================================
        // ==== PUBLIC ENDPOINT ====
        // ============================================================

        /// <summary>
        /// Check beschikbaarheid voor alle eenheden in een periode.
        /// Implementeert Parent-Child blokkade logica.
        /// </summary>
        /// <param name="startDatum">Check-in datum</param>
        /// <param name="eindDatum">Check-out datum</param>
        /// <param name="aantalPersonen">Optioneel: filter op capaciteit</param>
        /// <returns>Lijst van eenheden met IsBeschikbaar flag</returns>
        [HttpGet]
        public async Task<ActionResult<List<VerhuurEenheidDTO>>> CheckBeschikbaarheid(
            [FromQuery] DateTime startDatum,
            [FromQuery] DateTime eindDatum,
            [FromQuery] int? aantalPersonen = null)
        {
            // ==== Input Validatie ====
            if (eindDatum <= startDatum)
                return BadRequest("Einddatum moet na startdatum liggen.");

            var units = await _beschikbaarheidService.CheckBeschikbaarheidAsync(
                startDatum,
                eindDatum,
                aantalPersonen);

            return Ok(units);
        }
    }
}
