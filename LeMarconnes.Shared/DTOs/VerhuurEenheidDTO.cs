// ======== Imports ========
using System;

// ======== Namespace ========
namespace LeMarconnes.Shared.DTOs
{
    /// <summary>
    /// Data Transfer Object voor VERHUUR_EENHEID entiteit.
    /// Bevat de Parent-Child structuur via ParentEenheidID.
    /// 
    /// PARENT-CHILD STRUCTUUR:
    /// - EenheidID 1 = "Gîte Totaal" (Parent, TypeID=1)
    /// - EenheidID 2-10 = Slaapplekken (Children, TypeID=2, ParentEenheidID=1)
    /// 
    /// Correspondeert met de VERHUUR_EENHEID tabel in de database.
    /// </summary>
    public class VerhuurEenheidDTO
    {
        // ==== Properties ====
        // Database kolommen
        
        /// <summary>Primary Key - Unieke identifier van de eenheid</summary>
        public int EenheidID { get; set; }
        
        /// <summary>Naam van de eenheid (bijv. "Gîte Totaal" of "Slaapplek 1")</summary>
        public string Naam { get; set; } = string.Empty;
        
        /// <summary>
        /// Type van de eenheid (Foreign Key naar ACCOMMODATIE_TYPE)
        /// 1 = Geheel (de volledige Gîte)
        /// 2 = Slaapplek (individuele slaapkamer)
        /// </summary>
        public int TypeID { get; set; }
        
        /// <summary>Maximale capaciteit (aantal personen)</summary>
        public int MaxCapaciteit { get; set; }
        
        /// <summary>
        /// Parent eenheid ID (NULL voor de Parent zelf)
        /// Slaapplekken hebben ParentEenheidID = 1 (de Gîte Totaal)
        /// </summary>
        public int? ParentEenheidID { get; set; }
        
        // ==== Runtime Properties ====
        // Niet opgeslagen in database, berekend door business logic
        
        /// <summary>
        /// Runtime berekend veld - NIET opgeslagen in database.
        /// Wordt gezet door de beschikbaarheidslogica in GiteController.
        /// true = beschikbaar voor de gevraagde periode
        /// false = bezet of geblokkeerd door Parent-Child logica
        /// </summary>
        public bool IsBeschikbaar { get; set; } = true;

        // ==== Constructors ====
        
        /// <summary>
        /// Parameterloze constructor.
        /// Nodig voor JSON deserialisatie en database mapping.
        /// </summary>
        public VerhuurEenheidDTO() { }

        /// <summary>
        /// Constructor met alle database velden.
        /// </summary>
        /// <param name="eenheidId">Primary Key</param>
        /// <param name="naam">Naam van de eenheid</param>
        /// <param name="typeId">AccommodatieTypeID (1=Geheel, 2=Slaapplek)</param>
        /// <param name="maxCapaciteit">Maximale capaciteit</param>
        /// <param name="parentEenheidId">Parent ID (null voor parent zelf)</param>
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
