namespace LeMarconnes.Shared.DTOs;

/// <summary>
/// Request DTO voor login.
/// </summary>
public class LoginRequestDTO
{
    public string Email { get; set; } = string.Empty;
    public string Wachtwoord { get; set; } = string.Empty;
}
