// ======== Imports ========
using System;

// ======== Namespace ========
namespace LeMarconnes.Shared.DTOs
{
    /// <summary>
    /// Data Transfer Object voor TARIEF entiteit.
    /// Prijzen zijn historisch opgeslagen met GeldigVan/GeldigTot.
    /// 
    /// Correspondeert met de TARIEF tabel in de database.
    /// 
    /// PRIJS LOGICA:
    /// - TypeID 1 (Geheel): Prijs per nacht voor de hele Gîte
    /// - TypeID 2 (Slaapplek): Prijs per persoon per nacht
    /// 
    /// TOERISTENBELASTING:
    /// - TaxStatus = false: Prijs is EXCLUSIEF toeristenbelasting
    /// - TaxStatus = true: Prijs is INCLUSIEF toeristenbelasting
    /// - TaxTarief: Het belastingtarief per persoon per nacht
    /// </summary>
    public class TariefDTO
    {
        // ==== Properties ====
        // Database kolommen
        
        /// <summary>Primary Key - Auto increment in database</summary>
        public int TariefID { get; set; }
        
        /// <summary>
        /// Foreign Key naar ACCOMMODATIE_TYPE.
        /// 1 = Geheel (prijs per nacht)
        /// 2 = Slaapplek (prijs per persoon per nacht)
        /// </summary>
        public int TypeID { get; set; }
        
        /// <summary>
        /// Foreign Key naar TARIEF_CATEGORIE.
        /// Bijv: 1 = "Overnachting", 2 = "Schoonmaak"
        /// </summary>
        public int CategorieID { get; set; }
        
        /// <summary>
        /// Foreign Key naar PLATFORM (optioneel).
        /// NULL = algemeen tarief (geldt voor alle platformen)
        /// Specifiek PlatformID = platform-specifiek tarief
        /// </summary>
        public int? PlatformID { get; set; }
        
        /// <summary>De prijs (per nacht of per persoon per nacht)</summary>
        public decimal Prijs { get; set; }
        
        /// <summary>
        /// Toeristenbelasting status.
        /// false = Prijs is EXCLUSIEF belasting
        /// true = Prijs is INCLUSIEF belasting
        /// </summary>
        public bool TaxStatus { get; set; }
        
        /// <summary>Toeristenbelasting tarief per persoon per nacht</summary>
        public decimal TaxTarief { get; set; }
        
        /// <summary>Datum vanaf wanneer dit tarief geldig is</summary>
        public DateTime GeldigVan { get; set; }
        
        /// <summary>
        /// Datum tot wanneer dit tarief geldig is (optioneel).
        /// NULL = onbeperkt geldig (tot nieuw tarief wordt aangemaakt)
        /// </summary>
        public DateTime? GeldigTot { get; set; }

        // ==== Constructors ====
        
        /// <summary>
        /// Parameterloze constructor.
        /// Nodig voor JSON deserialisatie en database mapping.
        /// </summary>
        public TariefDTO() { }

        /// <summary>
        /// Constructor met minimale vereiste velden.
        /// </summary>
        /// <param name="tariefId">Primary Key</param>
        /// <param name="typeId">AccommodatieTypeID</param>
        /// <param name="categorieId">TariefCategorieID</param>
        /// <param name="prijs">De prijs</param>
        /// <param name="geldigVan">Startdatum geldigheid</param>
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
