// ======== Imports ========
using System;
using System.Text.Json.Serialization;

// ======== Namespace ========
namespace LeMarconnes.Shared.DTOs
{
    // DTO voor Gebruiker. WachtwoordHash wordt NOOIT naar client gestuurd.
    public class GebruikerDTO
    {
        // ==== Properties ====
        public int GebruikerID { get; set; }
        public int? GastID { get; set; }
        public string Email { get; set; } = string.Empty;
        [JsonIgnore]
        public string WachtwoordHash { get; set; } = string.Empty;
        public string Rol { get; set; } = "Gast";

        // ==== OOB (Relational) Properties ====
        public GastDTO? Gast { get; set; }

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
