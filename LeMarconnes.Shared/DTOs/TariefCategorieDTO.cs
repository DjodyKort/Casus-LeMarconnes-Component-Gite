// ======== Imports ========
using System;

// ======== Namespace ========
namespace LeMarconnes.Shared.DTOs
{
    // DTO voor TariefCategorie — lookup: 'Logies', 'Toeristenbelasting', etc.
    public class TariefCategorieDTO
    {
        // ==== Properties ====
        public int CategorieID { get; set; }
        public string Naam { get; set; } = string.Empty;

        // ==== Constructor ====
        public TariefCategorieDTO() { }

        public TariefCategorieDTO(int categorieId, string naam)
        {
            CategorieID = categorieId;
            Naam = naam;
        }
    }
}
