// ======== Imports ========
using System;

// ======== Namespace ========
namespace LeMarconnes.Shared.DTOs
{
    /// <summary>
    /// Response DTO die teruggestuurd wordt na een boeking poging.
    /// Bevat ofwel de bevestigingsdetails (bij succes) of een foutmelding (bij falen).
    /// 
    /// Dit DTO wordt door de API teruggestuurd naar de Console Client
    /// als response op POST /api/gite/boek
    /// 
    /// Gebruik de factory methods Success() en Failure() om instanties te maken.
    /// </summary>
    public class BoekingResponseDTO
    {
        // ==== Properties ====
        
        /// <summary>
        /// Het toegekende reserveringsnummer.
        /// Alleen gevuld bij succesvolle boeking.
        /// </summary>
        public int ReserveringID { get; set; }
        
        /// <summary>
        /// Bevestigingsbericht voor de gebruiker.
        /// Bijv: "Boeking bevestigd! Reserveringsnummer: 5"
        /// </summary>
        public string Bevestiging { get; set; } = string.Empty;
        
        /// <summary>Naam van de geboekte eenheid</summary>
        public string EenheidNaam { get; set; } = string.Empty;
        
        /// <summary>Startdatum van de reservering</summary>
        public DateTime StartDatum { get; set; }
        
        /// <summary>Einddatum van de reservering</summary>
        public DateTime EindDatum { get; set; }
        
        /// <summary>Berekende totaalprijs van de boeking</summary>
        public decimal TotaalPrijs { get; set; }
        
        /// <summary>
        /// Geeft aan of de boeking succesvol was.
        /// true = boeking is aangemaakt
        /// false = boeking is mislukt (zie FoutMelding)
        /// </summary>
        public bool Succes { get; set; }
        
        /// <summary>
        /// Foutmelding bij mislukte boeking.
        /// Alleen gevuld als Succes = false.
        /// </summary>
        public string? FoutMelding { get; set; }

        // ==== Constructor ====
        
        /// <summary>
        /// Parameterloze constructor.
        /// Gebruik liever de factory methods Success() en Failure().
        /// </summary>
        public BoekingResponseDTO() { }

        // ==== Factory Methods ====
        // Static methods om eenvoudig success/failure responses te maken
        
        /// <summary>
        /// Maakt een succesvolle boeking response.
        /// </summary>
        /// <param name="reserveringId">Het toegekende reserveringsnummer</param>
        /// <param name="eenheidNaam">Naam van de geboekte eenheid</param>
        /// <param name="start">Startdatum</param>
        /// <param name="eind">Einddatum</param>
        /// <param name="totaalPrijs">Berekende totaalprijs</param>
        /// <returns>Een BoekingResponseDTO met Succes=true</returns>
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

        /// <summary>
        /// Maakt een mislukte boeking response.
        /// </summary>
        /// <param name="foutMelding">Beschrijving van wat er mis ging</param>
        /// <returns>Een BoekingResponseDTO met Succes=false</returns>
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
