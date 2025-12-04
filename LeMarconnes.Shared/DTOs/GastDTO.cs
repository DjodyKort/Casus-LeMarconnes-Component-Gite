// ======== Imports ========
using System;

// ======== Namespace ========
namespace LeMarconnes.Shared.DTOs
{
    /// <summary>
    /// Data Transfer Object voor GAST entiteit.
    /// Bevat NAW-gegevens voor factuur en nachtregister.
    /// 
    /// Correspondeert met de GAST tabel in de database.
    /// Email is uniek en wordt gebruikt als identifier bij boekingen.
    /// </summary>
    public class GastDTO
    {
        // ==== Properties ====
        // Database kolommen
        
        /// <summary>Primary Key - Auto increment in database</summary>
        public int GastID { get; set; }
        
        /// <summary>Volledige naam van de gast</summary>
        public string Naam { get; set; } = string.Empty;
        
        /// <summary>Email adres (UNIEK) - Wordt gebruikt als identifier</summary>
        public string Email { get; set; } = string.Empty;
        
        /// <summary>Telefoonnummer (optioneel)</summary>
        public string? Tel { get; set; }
        
        /// <summary>Straatnaam zonder huisnummer</summary>
        public string Straat { get; set; } = string.Empty;
        
        /// <summary>Huisnummer inclusief eventuele toevoeging</summary>
        public string Huisnr { get; set; } = string.Empty;
        
        /// <summary>Postcode</summary>
        public string Postcode { get; set; } = string.Empty;
        
        /// <summary>Woonplaats</summary>
        public string Plaats { get; set; } = string.Empty;
        
        /// <summary>Land (default: Nederland)</summary>
        public string Land { get; set; } = "Nederland";
        
        /// <summary>IBAN bankrekeningnummer (optioneel, voor terugbetalingen)</summary>
        public string? IBAN { get; set; }

        // ==== Constructors ====
        
        /// <summary>
        /// Parameterloze constructor.
        /// Nodig voor JSON deserialisatie en object initializers.
        /// </summary>
        public GastDTO() { }

        /// <summary>
        /// Constructor met minimale vereiste velden.
        /// </summary>
        /// <param name="gastId">Het GastID</param>
        /// <param name="naam">Volledige naam</param>
        /// <param name="email">Email adres</param>
        public GastDTO(int gastId, string naam, string email)
        {
            GastID = gastId;
            Naam = naam;
            Email = email;
        }
    }
}
