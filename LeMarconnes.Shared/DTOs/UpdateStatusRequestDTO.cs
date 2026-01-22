namespace LeMarconnes.Shared.DTOs
{
    /// <summary>
    /// Request DTO voor het wijzigen van de status.
    /// </summary>
    public class UpdateStatusRequestDTO
    {
        public string Status { get; set; } = string.Empty;
    }
}
