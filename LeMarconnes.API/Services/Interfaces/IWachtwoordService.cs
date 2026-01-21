// ======== Namespace ========
namespace LeMarconnes.API.Services.Interfaces
{
    /// <summary>
    /// Interface voor wachtwoord hashing en verificatie.
    /// </summary>
    public interface IWachtwoordService
    {
        /// <summary>
        /// Hash een wachtwoord.
        /// </summary>
        /// <param name="wachtwoord">Plain text wachtwoord</param>
        /// <returns>Hashed wachtwoord</returns>
        string HashWachtwoord(string wachtwoord);

        /// <summary>
        /// Verificeer een wachtwoord tegen een hash.
        /// </summary>
        /// <param name="wachtwoord">Plain text wachtwoord</param>
        /// <param name="hash">Opgeslagen hash</param>
        /// <returns>True als wachtwoord correct is</returns>
        bool VerifyWachtwoord(string wachtwoord, string hash);
    }
}
