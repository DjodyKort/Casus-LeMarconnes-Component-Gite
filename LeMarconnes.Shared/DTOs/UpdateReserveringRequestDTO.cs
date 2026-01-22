using System;

namespace LeMarconnes.Shared.DTOs
{
    /// <summary>
    /// Request DTO voor het wijzigen van een reservering (alle velden optioneel).
    /// </summary>
    public class UpdateReserveringRequestDTO
    {
        public DateTime? StartDatum { get; set; }
        public DateTime? EindDatum { get; set; }
        public int? EenheidID { get; set; }
        public int? AantalPersonen { get; set; }
    }
}
