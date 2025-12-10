// ======== Imports ========
using System;

// ======== Namespace ========
namespace LeMarconnes.Shared.DTOs
{
    // DTO voor Tarief — bevat prijslogica en geldigheidsperiode.
    // TypeID 1 (Geheel): Prijs per nacht
    // TypeID 2 (Slaapplek): Prijs per persoon per nacht
    public class TariefDTO
    {
        // ==== Properties ====
        // Database kolommen
        
        // Primary Key
        public int TariefID { get; set; }
        
        // FK naar ACCOMMODATIE_TYPE (1=Geheel, 2=Slaapplek)
        public int TypeID { get; set; }
        
        // FK naar TARIEF_CATEGORIE (bijv. 1=Overnachting, 2=Schoonmaak)
        public int CategorieID { get; set; }
        
        // FK naar PLATFORM (null = algemeen tarief)
        public int? PlatformID { get; set; }
        
        // Prijs (per nacht of per persoon per nacht)
        public decimal Prijs { get; set; }
        
        // Toeristenbelasting status (false=Excl, true=Incl)
        public bool TaxStatus { get; set; }
        
        // Toeristenbelasting tarief per persoon per nacht
        public decimal TaxTarief { get; set; }
        
        // Geldigheid vanaf
        public DateTime GeldigVan { get; set; }
        
        // Geldigheid tot (null = onbeperkt)
        public DateTime? GeldigTot { get; set; }

        // ==== Constructors ====
        
        public TariefDTO() { }

        public TariefDTO(int tariefId, int typeId, int categorieId, decimal prijs, DateTime geldigVan)
        {
            TariefID = tariefId;
            TypeID = typeId;
            CategorieID = categorieId;
            Prijs = prijs;
            GeldigVan = geldigVan;
        }
    }
}
