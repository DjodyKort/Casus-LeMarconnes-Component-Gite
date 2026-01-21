// ======== Imports ========
using System;
using LeMarconnes.API.Services.Interfaces;

// ======== Namespace ========
namespace LeMarconnes.API.Services.Implementations
{
    /// <summary>
    /// Service voor wachtwoord hashing en verificatie.
    /// Centraliseert crypto-logica zodat het herbruikbaar is voor controllers, seed scripts, etc.
    /// </summary>
    public class WachtwoordService : IWachtwoordService
    {
        /// <summary>
        /// Hash wachtwoord met SHA256.
        /// TODO: Implementeer BCrypt.Net-Next package voor productie.
        /// </summary>
        /// <param name="wachtwoord">Plain text wachtwoord</param>
        /// <returns>Base64 encoded hash</returns>
        public string HashWachtwoord(string wachtwoord)
        {
            if (string.IsNullOrWhiteSpace(wachtwoord))
                throw new ArgumentException("Wachtwoord mag niet leeg zijn.", nameof(wachtwoord));

            // TIJDELIJK: simpele hash voor development
            // In productie: gebruik BCrypt.Net.BCrypt.HashPassword(wachtwoord)
            return Convert.ToBase64String(
                System.Security.Cryptography.SHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes(wachtwoord)));
        }

        /// <summary>
        /// Verificeer wachtwoord tegen hash.
        /// </summary>
        /// <param name="wachtwoord">Plain text wachtwoord</param>
        /// <param name="hash">Opgeslagen hash</param>
        /// <returns>True als wachtwoord correct is</returns>
        public bool VerifyWachtwoord(string wachtwoord, string hash)
        {
            if (string.IsNullOrWhiteSpace(wachtwoord) || string.IsNullOrWhiteSpace(hash))
                return false;

            // TIJDELIJK: simpele verificatie
            // In productie: gebruik BCrypt.Net.BCrypt.Verify(wachtwoord, hash)
            return HashWachtwoord(wachtwoord) == hash;
        }
    }
}
