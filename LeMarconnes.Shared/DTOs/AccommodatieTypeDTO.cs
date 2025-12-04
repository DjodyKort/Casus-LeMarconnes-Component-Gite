// ======== Imports ========
using System;

// ======== Namespace ========
namespace LeMarconnes.Shared.DTOs
{
    /// <summary>
    /// Data Transfer Object voor ACCOMMODATIE_TYPE entiteit.
    /// Lookup tabel: 'Gîte-Geheel' (1) of 'Gîte-Slaapplek' (2).
    /// </summary>
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
