// ======== Imports ========
using System;

// ======== Namespace ========
namespace LeMarconnes.Shared.DTOs
{
    /// <summary>
    /// Data Transfer Object voor PLATFORM entiteit.
    /// Boekingskanalen met hun commissiepercentages.
    /// 
    /// Correspondeert met de PLATFORM tabel in de database.
    /// 
    /// STANDAARD PLATFORMEN:
    /// - ID 1: "Eigen Site" (0% commissie)
    /// - ID 2: "Booking.com" (15% commissie)
    /// - ID 3: "Airbnb" (3% commissie)
    /// 
    /// De commissie wordt gebruikt voor omzetrapportages,
    /// niet voor directe prijsberekening aan de gast.
    /// </summary>
    public class PlatformDTO
    {
        // ==== Properties ====
        // Database kolommen
        
        /// <summary>Primary Key - Unieke identifier van het platform</summary>
        public int PlatformID { get; set; }
        
        /// <summary>Naam van het platform (bijv. "Booking.com")</summary>
        public string Naam { get; set; } = string.Empty;
        
        /// <summary>
        /// Commissiepercentage dat het platform inhoudt.
        /// 0 = Eigen Site (geen commissie)
        /// 15 = Booking.com (15% commissie)
        /// 3 = Airbnb (3% commissie)
        /// </summary>
        public decimal CommissiePercentage { get; set; }

        // ==== Constructors ====
        
        /// <summary>
        /// Parameterloze constructor.
        /// Nodig voor JSON deserialisatie en database mapping.
        /// </summary>
        public PlatformDTO() { }

        /// <summary>
        /// Constructor met alle velden.
        /// </summary>
        /// <param name="platformId">Primary Key</param>
        /// <param name="naam">Naam van het platform</param>
        /// <param name="commissiePercentage">Commissiepercentage</param>
        public PlatformDTO(int platformId, string naam, decimal commissiePercentage)
        {
            PlatformID = platformId;
            Naam = naam;
            CommissiePercentage = commissiePercentage;
        }
    }
}
