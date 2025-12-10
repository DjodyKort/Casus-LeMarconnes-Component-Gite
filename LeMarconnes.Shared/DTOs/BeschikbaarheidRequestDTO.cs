// ======== Imports ========
using System;

// ======== Namespace ========
namespace LeMarconnes.Shared.DTOs
{
    // Request DTO voor beschikbaarheidscheck binnen een periode.
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
