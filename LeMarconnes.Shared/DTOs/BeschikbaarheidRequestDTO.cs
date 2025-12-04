// ======== Imports ========
using System;

// ======== Namespace ========
namespace LeMarconnes.Shared.DTOs
{
    /// <summary>
    /// Request DTO voor beschikbaarheidscheck.
    /// Wordt door de client naar de API gestuurd om te checken
    /// welke eenheden vrij zijn in een bepaalde periode.
    /// </summary>
    public class BeschikbaarheidRequestDTO
    {
        // ==== Properties ====
        public DateTime StartDatum { get; set; }
        public DateTime EindDatum { get; set; }

        // ==== Constructor ====
        public BeschikbaarheidRequestDTO() { }

        public BeschikbaarheidRequestDTO(DateTime startDatum, DateTime eindDatum)
        {
            StartDatum = startDatum;
            EindDatum = eindDatum;
        }
    }
}
