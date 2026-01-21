namespace LeMarconnes.Shared.DTOs;

/// <summary>
/// Request DTO voor het wijzigen van wachtwoord.
/// </summary>
public class UpdateWachtwoordRequestDTO
{
    public string OudWachtwoord { get; set; } = string.Empty;
    public string NieuwWachtwoord { get; set; } = string.Empty;
}
