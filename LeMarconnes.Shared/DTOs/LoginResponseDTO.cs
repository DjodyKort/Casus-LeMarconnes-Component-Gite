namespace LeMarconnes.Shared.DTOs;

/// <summary>
/// Response DTO voor login.
/// </summary>
public class LoginResponseDTO
{
    public bool Succes { get; set; }
    public string Bericht { get; set; } = string.Empty;
    public GebruikerDTO? Gebruiker { get; set; }
    // TODO: Voeg JWT token toe wanneer geïmplementeerd
    // public string Token { get; set; } = string.Empty;
    // public DateTime ExpiresAt { get; set; }
}
