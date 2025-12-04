// ======== Imports ========
using System;

// ======== Namespace ========
namespace LeMarconnes.Shared.DTOs
{
    /// <summary>
    /// Data Transfer Object voor TARIEF_CATEGORIE entiteit.
    /// Categorieën: 'Logies', 'Toeristenbelasting'.
    /// </summary>
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
