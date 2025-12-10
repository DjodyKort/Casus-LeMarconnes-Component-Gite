// ======== Imports ========
using System;

// ======== Namespace ========
namespace LeMarconnes.Shared.DTOs
{
    // DTO voor Platform — boekingskanalen met commissiepercentages.
    // ID 1: "Eigen Site" (0%), ID 2: "Booking.com" (15%), ID 3: "Airbnb" (3%)
    public class PlatformDTO
    {
        // ==== Properties ====
        // Database kolommen
        
        // Primary Key
        public int PlatformID { get; set; }
        
        // Naam van het platform
        public string Naam { get; set; } = string.Empty;
        
        // Commissiepercentage
        public decimal CommissiePercentage { get; set; }

        // ==== Constructors ====
        
        public PlatformDTO() { }

        public PlatformDTO(int platformId, string naam, decimal commissiePercentage)
        {
            PlatformID = platformId;
            Naam = naam;
            CommissiePercentage = commissiePercentage;
        }
    }
}
