// ======== Imports ========
using System;

// ======== Namespace ========
namespace LeMarconnes.Shared.DTOs
{
    // DTO voor Reservering: koppelt Gast, Eenheid en Periode.
    public class ReserveringDTO
    {
        // ==== Properties ====
        public int ReserveringID { get; set; }
        
        // FK naar Gast
        public int GastID { get; set; }
        
        // FK naar Eenheid
        public int EenheidID { get; set; }
        
        // FK naar Platform
        public int PlatformID { get; set; }
        public DateTime Startdatum { get; set; }
        public DateTime Einddatum { get; set; }
        
        // Status (Gereserveerd/Ingecheckt/Uitgecheckt/Geannuleerd)
        public string Status { get; set; } = "Gereserveerd";

        // ==== OOB (Relational) Properties ====
        public GastDTO? Gast { get; set; }
        public VerhuurEenheidDTO? Eenheid { get; set; }
        public PlatformDTO? Platform { get; set; }

        // ==== Constructors ====
        public ReserveringDTO() { }

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
