// ======== Imports ========
using System;

// ======== Namespace ========
namespace LeMarconnes.Shared.DTOs
{
    // DTO voor Logboek — audit trail voor alle mutaties.
    // Automatisch aangemaakt bij reserveringen, checks, wijzigingen.
    public class LogboekDTO
    {
        // ==== Properties ====
        public int LogID { get; set; }
        
        // FK naar GEBRUIKER (null als systeem de actie uitvoerde)
        public int? GebruikerID { get; set; }
        
        // Tijdstip van actie
        public DateTime Tijdstip { get; set; }
        
        // Beschrijving (bijv. "RESERVERING_AANGEMAAKT")
        public string Actie { get; set; } = string.Empty;
        
        // Tabel waarop actie werd uitgevoerd (optioneel)
        public string? TabelNaam { get; set; }
        
        // ID van record waarop actie werd uitgevoerd (optioneel)
        public int? RecordID { get; set; }
        
        // Oude waarde bij wijziging (JSON, optioneel)
        public string? OudeWaarde { get; set; }
        
        // Nieuwe waarde bij wijziging (JSON, optioneel)
        public string? NieuweWaarde { get; set; }

        // ==== OOB (Relational) Properties ====
        public GebruikerDTO? Gebruiker { get; set; }

        // ==== Constructors ====
        public LogboekDTO() 
        {
            Tijdstip = DateTime.Now;
        }

        public LogboekDTO(string actie, string? tabelNaam = null, int? recordId = null)
        {
            Actie = actie;
            TabelNaam = tabelNaam;
            RecordID = recordId;
            Tijdstip = DateTime.Now;
        }
    }
}
