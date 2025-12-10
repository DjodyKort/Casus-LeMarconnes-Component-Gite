// ======== Imports ========
using System;

// ======== Namespace ========
namespace LeMarconnes.Shared.DTOs
{
    // DTO voor VerhuurEenheid. Parent-child structuur via ParentEenheidID.
    public class VerhuurEenheidDTO
    {
        // ==== Properties ====
        // Database kolommen
        
        // Primary Key
        public int EenheidID { get; set; }
        
        // Naam van de eenheid
        public string Naam { get; set; } = string.Empty;
        
        // Accommodatie type (1=Geheel, 2=Slaapplek)
        public int TypeID { get; set; }
        
        // Maximale capaciteit
        public int MaxCapaciteit { get; set; }
        
        // Parent eenheid ID (null voor parent zelf)
        public int? ParentEenheidID { get; set; }
        
        // ==== Runtime Properties ====
        // Niet opgeslagen in DB; berekend door business logic
        public bool IsBeschikbaar { get; set; } = true;

        // ==== Constructors ====
        
        public VerhuurEenheidDTO() { }

        public VerhuurEenheidDTO(int eenheidId, string naam, int typeId, int maxCapaciteit, int? parentEenheidId)
        {
            EenheidID = eenheidId;
            Naam = naam;
            TypeID = typeId;
            MaxCapaciteit = maxCapaciteit;
            ParentEenheidID = parentEenheidId;
        }
    }
}
