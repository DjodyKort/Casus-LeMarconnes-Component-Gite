// ======== Imports ========
using System;

// ======== Namespace ========
namespace LeMarconnes.Shared.DTOs
{
    /// <summary>
    /// Data Transfer Object voor GEBRUIKER entiteit.
    /// Let op: WachtwoordHash wordt NOOIT meegestuurd naar client!
    /// </summary>
    public class GebruikerDTO
    {
        // ==== Properties ====
        public int GebruikerID { get; set; }
        public int? GastID { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Rol { get; set; } = "Gast";

        // ==== Constructor ====
        public GebruikerDTO() { }

        public GebruikerDTO(int gebruikerId, string email, string rol)
        {
            GebruikerID = gebruikerId;
            Email = email;
            Rol = rol;
        }
    }
}
