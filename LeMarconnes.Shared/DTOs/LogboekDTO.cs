// ======== Imports ========
using System;

// ======== Namespace ========
namespace LeMarconnes.Shared.DTOs
{
    /// <summary>
    /// Data Transfer Object voor LOGBOEK entiteit.
    /// Registratie van alle mutaties voor audit trail.
    /// 
    /// Correspondeert met de LOGBOEK tabel in de database.
    /// 
    /// Dit wordt automatisch aangemaakt door de API bij:
    /// - Beschikbaarheid checks
    /// - Nieuwe reserveringen
    /// - Annuleringen
    /// - Gast wijzigingen
    /// 
    /// Requirement: S-NF-SYS-003 (Audit Trail)
    /// </summary>
    public class LogboekDTO
    {
        // ==== Properties ====
        // Database kolommen
        
        /// <summary>Primary Key - Auto increment in database</summary>
        public int LogID { get; set; }
        
        /// <summary>
        /// Foreign Key naar GEBRUIKER (optioneel).
        /// NULL als de actie door het systeem is uitgevoerd.
        /// </summary>
        public int? GebruikerID { get; set; }
        
        /// <summary>Tijdstip van de actie (default: DateTime.Now)</summary>
        public DateTime Tijdstip { get; set; }
        
        /// <summary>
        /// Beschrijving van de uitgevoerde actie.
        /// Bijv: "RESERVERING_AANGEMAAKT", "BESCHIKBAARHEID_CHECK"
        /// </summary>
        public string Actie { get; set; } = string.Empty;
        
        /// <summary>
        /// Naam van de tabel waarop de actie is uitgevoerd (optioneel).
        /// Bijv: "RESERVERING", "GAST", "VERHUUR_EENHEID"
        /// </summary>
        public string? TabelNaam { get; set; }
        
        /// <summary>
        /// ID van het record waarop de actie is uitgevoerd (optioneel).
        /// Bijv: ReserveringID bij een reservering actie.
        /// </summary>
        public int? RecordID { get; set; }
        
        /// <summary>Oude waarde bij een wijziging (optioneel, JSON format)</summary>
        public string? OudeWaarde { get; set; }
        
        /// <summary>Nieuwe waarde bij een wijziging (optioneel, JSON format)</summary>
        public string? NieuweWaarde { get; set; }

        // ==== Constructors ====
        
        /// <summary>
        /// Parameterloze constructor.
        /// Zet automatisch Tijdstip op DateTime.Now.
        /// </summary>
        public LogboekDTO() 
        {
            Tijdstip = DateTime.Now;
        }

        /// <summary>
        /// Constructor voor het snel aanmaken van een log entry.
        /// Tijdstip wordt automatisch op DateTime.Now gezet.
        /// </summary>
        /// <param name="actie">Beschrijving van de actie</param>
        /// <param name="tabelNaam">Naam van de tabel (optioneel)</param>
        /// <param name="recordId">ID van het betreffende record (optioneel)</param>
        public LogboekDTO(string actie, string? tabelNaam = null, int? recordId = null)
        {
            Actie = actie;
            TabelNaam = tabelNaam;
            RecordID = recordId;
            Tijdstip = DateTime.Now;
        }
    }
}
