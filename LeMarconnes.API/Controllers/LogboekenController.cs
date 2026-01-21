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
    /// Controller voor audit trail (logboek).
    /// Read-only endpoints voor admin.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class LogboekenController : ControllerBase
    {
        // ==== Properties ====
        private readonly IGiteRepository _repository;

        // ==== Constructor ====
        public LogboekenController(IGiteRepository repository)
        {
            _repository = repository;
        }

        // ============================================================
        // ==== ADMIN ENDPOINTS ====
        // ============================================================

        /// <summary>
        /// Haal recente logboek entries op (admin only).
        /// </summary>
        /// <param name="count">Aantal entries (default: 50, max: 500)</param>
        [HttpGet]
        public async Task<ActionResult<List<LogboekDTO>>> GetRecent([FromQuery] int count = 50)
        {
            // Limiteer tot max 500
            if (count > 500)
                count = 500;

            var logs = await _repository.GetRecentLogsAsync(count);
            return Ok(logs);
        }

        /// <summary>
        /// Haal logboek entries op voor een specifieke entiteit (admin only).
        /// Toont volledige geschiedenis van een record.
        /// </summary>
        /// <param name="type">Tabel naam (RESERVERING, GAST, VERHUUR_EENHEID, etc)</param>
        /// <param name="id">Record ID</param>
        [HttpGet("entiteit/{type}/{id}")]
        public async Task<ActionResult<List<LogboekDTO>>> GetByEntiteit(string type, int id)
        {
            var logs = await _repository.GetLogsByEntiteitAsync(type, id);
            return Ok(logs);
        }
    }
}
