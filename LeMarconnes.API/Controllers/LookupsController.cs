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
    /// Controller voor read-only lookups: platformen en tarief categorieën.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class LookupsController : ControllerBase
    {
        // ==== Properties ====
        private readonly IGiteRepository _repository;

        // ==== Constructor ====
        public LookupsController(IGiteRepository repository)
        {
            _repository = repository;
        }

        // ============================================================
        // ==== PLATFORMEN ====
        // ============================================================

        /// <summary>
        /// Haal alle platformen op (Eigen Site, Booking.com, Airbnb).
        /// </summary>
        [HttpGet("platformen")]
        public async Task<ActionResult<List<PlatformDTO>>> GetAllPlatformen()
        {
            var platforms = await _repository.GetAllPlatformsAsync();
            return Ok(platforms);
        }

        /// <summary>
        /// Haal een specifiek platform op.
        /// </summary>
        /// <param name="id">Platform ID</param>
        [HttpGet("platformen/{id}")]
        public async Task<ActionResult<PlatformDTO>> GetPlatformById(int id)
        {
            var platform = await _repository.GetPlatformByIdAsync(id);

            if (platform == null)
                return NotFound($"Platform met ID {id} niet gevonden.");

            return Ok(platform);
        }

        // ============================================================
        // ==== TARIEF CATEGORIEËN ====
        // ============================================================

        /// <summary>
        /// Haal alle tarief categorieën op (Logies, Toeristenbelasting).
        /// </summary>
        [HttpGet("tariefcategorieen")]
        public async Task<ActionResult<List<TariefCategorieDTO>>> GetAllTariefCategorieen()
        {
            var categorieen = await _repository.GetAllTariefCategoriesAsync();
            return Ok(categorieen);
        }
    }
}
