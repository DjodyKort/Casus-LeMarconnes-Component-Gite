// ======== Imports ========
using System;

// ======== Namespace ========
namespace LeMarconnes.Shared.DTOs
{
    /// <summary>
    /// Data Transfer Object voor RESERVERING entiteit.
    /// De kernkoppeling tussen Gast, Eenheid en Periode.
    /// 
    /// Correspondeert met de RESERVERING tabel in de database.
    /// 
    /// STATUS WAARDEN:
    /// - "Gereserveerd" = Boeking aangemaakt, nog niet ingecheckt
    /// - "Ingecheckt" = Gast is gearriveerd
    /// - "Uitgecheckt" = Gast is vertrokken
    /// - "Geannuleerd" = Boeking is geannuleerd (soft delete)
    /// </summary>
    public class ReserveringDTO
    {
        // ==== Properties ====
        // Database kolommen
        
        /// <summary>Primary Key - Auto increment in database</summary>
        public int ReserveringID { get; set; }
        
        /// <summary>Foreign Key naar GAST tabel</summary>
        public int GastID { get; set; }
        
        /// <summary>Foreign Key naar VERHUUR_EENHEID tabel</summary>
        public int EenheidID { get; set; }
        
        /// <summary>Foreign Key naar PLATFORM tabel (Booking.com, Airbnb, etc.)</summary>
        public int PlatformID { get; set; }
        
        /// <summary>Startdatum van de reservering (check-in dag)</summary>
        public DateTime Startdatum { get; set; }
        
        /// <summary>Einddatum van de reservering (check-out dag)</summary>
        public DateTime Einddatum { get; set; }
        
        /// <summary>
        /// Status van de reservering.
        /// Mogelijke waarden: "Gereserveerd", "Ingecheckt", "Uitgecheckt", "Geannuleerd"
        /// </summary>
        public string Status { get; set; } = "Gereserveerd";

        // ==== Constructors ====
        
        /// <summary>
        /// Parameterloze constructor.
        /// Nodig voor JSON deserialisatie en database mapping.
        /// </summary>
        public ReserveringDTO() { }

        /// <summary>
        /// Constructor met alle vereiste velden voor een nieuwe reservering.
        /// </summary>
        /// <param name="gastId">GastID van de boeker</param>
        /// <param name="eenheidId">EenheidID van de geboekte eenheid</param>
        /// <param name="platformId">PlatformID waarop geboekt is</param>
        /// <param name="startdatum">Check-in datum</param>
        /// <param name="einddatum">Check-out datum</param>
        public ReserveringDTO(int gastId, int eenheidId, int platformId, DateTime startdatum, DateTime einddatum)
        {
            GastID = gastId;
            EenheidID = eenheidId;
            PlatformID = platformId;
            Startdatum = startdatum;
            Einddatum = einddatum;
        }
    }
}
