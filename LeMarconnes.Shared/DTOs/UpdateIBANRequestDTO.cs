// ======== Imports ========
using System;

// ======== Namespace ========
namespace LeMarconnes.Shared.DTOs
{
    /// <summary>
    /// Request DTO voor het toevoegen van IBAN aan een gast via betaalwebhook.
    /// Wordt aangeroepen vanuit de betaalprovider (Mollie, Stripe, etc.)
    /// </summary>
    public class UpdateIBANRequestDTO
    {
        /// <summary>
        /// ID van de gast waaraan het IBAN moet worden gekoppeld
        /// </summary>
        public int GastID { get; set; }

        /// <summary>
        /// IBAN van de gast (verkregen via betaalprovider)
        /// </summary>
        public string IBAN { get; set; } = string.Empty;

        // ==== Constructor ====
        public UpdateIBANRequestDTO() { }

        public UpdateIBANRequestDTO(int gastId, string iban)
        {
            GastID = gastId;
            IBAN = iban;
        }
    }
}
