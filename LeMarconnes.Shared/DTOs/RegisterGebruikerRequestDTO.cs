// ======== Imports ========
using System;

// ======== Namespace ========
namespace LeMarconnes.Shared.DTOs
{
    /// <summary>
    /// Request DTO voor het registreren van een nieuwe gebruiker.
    /// Bevat alleen email en wachtwoord. NAW gegevens worden later toegevoegd bij eerste reservering.
    /// </summary>
    public class RegisterGebruikerRequestDTO
    {
        /// <summary>
        /// Email adres (wordt gebruikersnaam en moet uniek zijn)
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Wachtwoord in plaintext (wordt gehashed door backend)
        /// </summary>
        public string Wachtwoord { get; set; } = string.Empty;

        // ==== Constructor ====
        public RegisterGebruikerRequestDTO() { }

        public RegisterGebruikerRequestDTO(string email, string wachtwoord)
        {
            Email = email;
            Wachtwoord = wachtwoord;
        }
    }
}
