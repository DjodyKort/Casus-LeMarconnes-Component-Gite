// ======== Imports ========
using System;

// ======== Namespace ========
namespace LeMarconnes.Shared.DTOs
{
    // DTO voor AccommodatieType — lookup: 'Gîte-Geheel' (1), 'Gîte-Slaapplek' (2).
    public class AccommodatieTypeDTO
    {
        // ==== Properties ====
        public int TypeID { get; set; }
        public string Naam { get; set; } = string.Empty;

        // ==== Constructor ====
        public AccommodatieTypeDTO() { }

        public AccommodatieTypeDTO(int typeId, string naam)
        {
            TypeID = typeId;
            Naam = naam;
        }
    }
}
