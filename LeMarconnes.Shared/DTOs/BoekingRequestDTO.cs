// ======== Imports ========
using System;

// ======== Namespace ========
namespace LeMarconnes.Shared.DTOs
{
    // Request DTO voor nieuwe boeking. Bevat gastgegevens + reserveringsdetails.
    // Gebruikt door Console Client via POST /api/gite/boek
    public class BoekingRequestDTO
    {
        // ============================================================
        // ==== GAST INFORMATIE ====
        // Wordt gebruikt om gast te zoeken of aan te maken
        // ============================================================
        
        // Volledige naam van de gast
        public string GastNaam { get; set; } = string.Empty;
        
        // Email (unieke identifier)
        public string GastEmail { get; set; } = string.Empty;
        
        // Telefoonnummer (optioneel)
        public string? GastTel { get; set; }
        
        // Straatnaam
        public string GastStraat { get; set; } = string.Empty;
        
        // Huisnummer
        public string GastHuisnr { get; set; } = string.Empty;
        
        // Postcode
        public string GastPostcode { get; set; } = string.Empty;
        
        // Woonplaats
        public string GastPlaats { get; set; } = string.Empty;
        
        // Land (default Nederland)
        public string GastLand { get; set; } = "Nederland";

        // ============================================================
        // ==== BOEKING INFORMATIE ====
        // Gegevens over de daadwerkelijke reservering
        // ============================================================
        
        // ID van de te boeken eenheid
        public int EenheidID { get; set; }
        
        // ID van het platform (1=Eigen, 2=Booking.com, 3=Airbnb)
        public int PlatformID { get; set; }
        
        // Startdatum (check-in)
        public DateTime StartDatum { get; set; }
        
        // Einddatum (check-out)
        public DateTime EindDatum { get; set; }
        
        // Aantal personen (voor prijsberekening slaappleken)
        public int AantalPersonen { get; set; } = 1;

        // ==== Constructor ====
        public BoekingRequestDTO() { }
    }
}
