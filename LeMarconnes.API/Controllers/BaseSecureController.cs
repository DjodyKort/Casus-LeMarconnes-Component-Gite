// ======== Imports ========
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LeMarconnes.API.DAL.Interfaces;
using LeMarconnes.Shared.DTOs;

// ======== Namespace ========
namespace LeMarconnes.API.Controllers
{
    /// <summary>
    /// Base controller met gedeelde security/authorization logica.
    /// Voorkomt duplicate code in child controllers.
    /// </summary>
    public abstract class BaseSecureController : ControllerBase
    {
        protected readonly IGiteRepository _repository;

        protected BaseSecureController(IGiteRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Haal de ingelogde gebruiker op op basis van email in claims.
        /// </summary>
        protected async Task<GebruikerDTO?> GetCurrentGebruikerAsync()
        {
            var email = User.Identity?.Name;
            if (string.IsNullOrEmpty(email))
                return null;

            return await _repository.GetGebruikerByEmailAsync(email);
        }

        /// <summary>
        /// Controleer of de ingelogde user toegang heeft tot de gegeven GastID.
        /// Admin heeft altijd toegang, User alleen tot eigen data.
        /// </summary>
        protected async Task<bool> CanAccessGastDataAsync(int gastId)
        {
            // Admin mag alles
            if (User.IsInRole("Admin"))
                return true;

            // User mag alleen eigen data
            var gebruiker = await GetCurrentGebruikerAsync();
            return gebruiker?.GastID == gastId;
        }

        /// <summary>
        /// Controleer of de ingelogde user de owner is van een reservering.
        /// </summary>
        protected async Task<bool> IsOwnerOfReserveringAsync(int reserveringId)
        {
            // Admin mag alles
            if (User.IsInRole("Admin"))
                return true;

            var reservering = await _repository.GetReserveringByIdAsync(reserveringId);
            if (reservering == null)
                return false;

            var gebruiker = await GetCurrentGebruikerAsync();
            return gebruiker?.GastID == reservering.GastID;
        }

        /// <summary>
        /// Maak een audit log entry aan.
        /// Helper method om code overzichtelijk te houden.
        /// </summary>
        protected async Task LogActionAsync(string actie, string entiteitType, int? entiteitId = null)
        {
            await _repository.CreateLogEntryAsync(
                new LogboekDTO(actie, entiteitType, entiteitId));
        }
    }
}
