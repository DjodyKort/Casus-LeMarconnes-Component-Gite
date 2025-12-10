// ======== Imports ========
using System;

// ======== Namespace ========
namespace LeMarconnes.Shared.DTOs
{
    // Response DTO voor boekingpoging — bevat bevestigingsdetails of foutmelding.
    // Gebruik factory methods Success() en Failure() om instanties te maken.
    public class BoekingResponseDTO
    {
        // ==== Properties ====
        
        // Het toegekende reserveringsnummer (alleen bij succes)
        public int ReserveringID { get; set; }
        
        // Bevestigingsbericht voor de gebruiker
        public string Bevestiging { get; set; } = string.Empty;
        
        // Naam van de geboekte eenheid
        public string EenheidNaam { get; set; } = string.Empty;
        
        // Startdatum
        public DateTime StartDatum { get; set; }
        
        // Einddatum
        public DateTime EindDatum { get; set; }
        
        // Berekende totaalprijs
        public decimal TotaalPrijs { get; set; }
        
        // true = boeking succesvol, false = mislukt (zie FoutMelding)
        public bool Succes { get; set; }
        
        // Foutmelding (alleen als Succes=false)
        public string? FoutMelding { get; set; }

        // ==== Constructor ====
        
        public BoekingResponseDTO() { }

        // ==== Factory Methods ====
        // Static methods om eenvoudig success/failure responses te maken
        
        // Maakt een succesvolle boeking response
        public static BoekingResponseDTO Success(int reserveringId, string eenheidNaam, DateTime start, DateTime eind, decimal totaalPrijs)
        {
            return new BoekingResponseDTO
            {
                ReserveringID = reserveringId,
                Bevestiging = $"Boeking bevestigd! Reserveringsnummer: {reserveringId}",
                EenheidNaam = eenheidNaam,
                StartDatum = start,
                EindDatum = eind,
                TotaalPrijs = totaalPrijs,
                Succes = true
            };
        }

        // Maakt een mislukte boeking response
        public static BoekingResponseDTO Failure(string foutMelding)
        {
            return new BoekingResponseDTO
            {
                Succes = false,
                FoutMelding = foutMelding
            };
        }
    }
}
