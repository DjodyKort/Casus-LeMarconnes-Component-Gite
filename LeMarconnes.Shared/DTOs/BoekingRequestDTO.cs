// ======== Imports ========
using System;

// ======== Namespace ========
namespace LeMarconnes.Shared.DTOs
{
    /// <summary>
    /// Request DTO voor het aanmaken van een nieuwe boeking.
    /// Bevat alle benodigde informatie om een reservering te maken,
    /// inclusief gastgegevens (voor het geval de gast nog niet bestaat).
    /// 
    /// Dit DTO wordt door de Console Client naar de API gestuurd
    /// via POST /api/gite/boek
    /// 
    /// De API zal:
    /// 1. Beschikbaarheid controleren
    /// 2. Gast zoeken op email, of nieuwe gast aanmaken
    /// 3. Tarief ophalen
    /// 4. Prijs berekenen
    /// 5. Reservering aanmaken
    /// </summary>
    public class BoekingRequestDTO
    {
        // ============================================================
        // ==== GAST INFORMATIE ====
        // Wordt gebruikt om gast te zoeken of aan te maken
        // ============================================================
        
        /// <summary>Volledige naam van de gast</summary>
        public string GastNaam { get; set; } = string.Empty;
        
        /// <summary>Email adres (wordt gebruikt als unieke identifier)</summary>
        public string GastEmail { get; set; } = string.Empty;
        
        /// <summary>Telefoonnummer (optioneel)</summary>
        public string? GastTel { get; set; }
        
        /// <summary>Straatnaam</summary>
        public string GastStraat { get; set; } = string.Empty;
        
        /// <summary>Huisnummer</summary>
        public string GastHuisnr { get; set; } = string.Empty;
        
        /// <summary>Postcode</summary>
        public string GastPostcode { get; set; } = string.Empty;
        
        /// <summary>Woonplaats</summary>
        public string GastPlaats { get; set; } = string.Empty;
        
        /// <summary>Land (default: Nederland)</summary>
        public string GastLand { get; set; } = "Nederland";

        // ============================================================
        // ==== BOEKING INFORMATIE ====
        // Gegevens over de daadwerkelijke reservering
        // ============================================================
        
        /// <summary>ID van de te boeken eenheid</summary>
        public int EenheidID { get; set; }
        
        /// <summary>
        /// ID van het platform waarop geboekt wordt.
        /// 1 = Eigen Site (0% commissie)
        /// 2 = Booking.com (15% commissie)
        /// 3 = Airbnb (3% commissie)
        /// </summary>
        public int PlatformID { get; set; }
        
        /// <summary>Startdatum van de reservering (check-in)</summary>
        public DateTime StartDatum { get; set; }
        
        /// <summary>Einddatum van de reservering (check-out)</summary>
        public DateTime EindDatum { get; set; }
        
        /// <summary>
        /// Aantal personen.
        /// Wordt gebruikt voor prijsberekening bij slaapplekken (per persoon per nacht).
        /// </summary>
        public int AantalPersonen { get; set; } = 1;

        // ==== Constructor ====<|disc_score|>1
        /// <summary>
        /// Parameterloze constructor.
        /// Nodig voor JSON deserialisatie.
        /// </summary>
        public BoekingRequestDTO() { }
    }
}
