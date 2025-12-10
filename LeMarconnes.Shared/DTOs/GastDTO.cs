// ======== Imports ========
using System;

// ======== Namespace ========
namespace LeMarconnes.Shared.DTOs
{
    // DTO voor Gast — bevat NAW gegevens en optionele betaalgegevens.
    public class GastDTO
    {
        // ==== Properties ====
        // Database kolommen
        
        // Primary Key
        public int GastID { get; set; }
        
        // Volledige naam van de gast
        public string Naam { get; set; } = string.Empty;
        
        // Email adres (uniek)
        public string Email { get; set; } = string.Empty;
        
        // Telefoonnummer (optioneel)
        public string? Tel { get; set; }
        
        // Straatnaam
        public string Straat { get; set; } = string.Empty;
        
        // Huisnummer inclusief toevoeging
        public string Huisnr { get; set; } = string.Empty;
        
        // Postcode
        public string Postcode { get; set; } = string.Empty;
        
        // Woonplaats
        public string Plaats { get; set; } = string.Empty;
        
        // Land (default Nederland)
        public string Land { get; set; } = "Nederland";
        
        // IBAN (optioneel)
        public string? IBAN { get; set; }

        // ==== Constructors ====
        
        // Parameterloze constructor
        public GastDTO() { }

        // Constructor met minimale velden
        public GastDTO(int gastId, string naam, string email)
        {
            GastID = gastId;
            Naam = naam;
            Email = email;
        }
    }
}
