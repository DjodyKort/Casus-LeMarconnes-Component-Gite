// ======== Imports ========
using System;
using System.Text.Json.Serialization;

// ======== Namespace ========
namespace LeMarconnes.Shared.DTOs
{
    // DTO voor VerhuurEenheid. Parent-child structuur via ParentEenheidID.
    public class VerhuurEenheidDTO
    {
        // ==== Properties ====       
        public int EenheidID { get; set; }
        
        public string Naam { get; set; } = string.Empty;

        // FK naar ACCOMMODATIE_TYPE (1=Geheel, 2=Slaapplek)
        public int TypeID { get; set; }
        
        public int MaxCapaciteit { get; set; }
        
        // (null voor parent zelf)
        public int? ParentEenheidID { get; set; }

        // ==== OOB (Relational) Properties ====
        public AccommodatieTypeDTO? Type { get; set; }

        [JsonIgnore]
        public VerhuurEenheidDTO? ParentEenheid { get; set; }
        public List<VerhuurEenheidDTO> ChildEenheden { get; set; } = new();

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
