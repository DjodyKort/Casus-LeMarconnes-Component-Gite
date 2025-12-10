// ======== Imports ========
using System;

// ======== Namespace ========
namespace LeMarconnes.Shared.DTOs
{
    // DTO voor ReserveringDetail — kostenposten binnen een reservering.
    // PrijsOpMoment: historische vastlegging van prijs tijdens boeking.
    public class ReserveringDetailDTO
    {
        // ==== Properties ====
        public int DetailID { get; set; }
        public int ReserveringID { get; set; }
        public int CategorieID { get; set; }
        public int Aantal { get; set; } = 1;
        public decimal PrijsOpMoment { get; set; }

        // ==== Constructor ====
        public ReserveringDetailDTO() { }

        public ReserveringDetailDTO(int reserveringId, int categorieId, int aantal, decimal prijsOpMoment)
        {
            ReserveringID = reserveringId;
            CategorieID = categorieId;
            Aantal = aantal;
            PrijsOpMoment = prijsOpMoment;
        }
    }
}
