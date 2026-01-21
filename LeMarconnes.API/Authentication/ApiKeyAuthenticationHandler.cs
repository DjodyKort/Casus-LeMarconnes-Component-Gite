// ======== Imports ========
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

// ======== Namespace ========
namespace LeMarconnes.API.Authentication
{
    /// <summary>
    /// API Key authenticatie handler.
    /// Controleert X-API-Key header en zet role (Admin of User) in claims.
    /// </summary>
    public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        // ==== Constants ====
        private const string ApiKeyHeaderName = "X-API-Key";
        private static readonly Dictionary<string, string> ApiKeys = new()
        {
            {"admin-key-12345", "Admin"},
            {"user-key-67890", "User"}
        };

        // ==== Constructor ====
        public ApiKeyAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {}

        // ==== Methods ====
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // ==== Checks ====
            if (!Request.Headers.TryGetValue(ApiKeyHeaderName, out var apiKeyHeaderValues)) {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            // ==== Declaring Variables ====
            var providedApiKey = apiKeyHeaderValues.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(providedApiKey)) {return Task.FromResult(AuthenticateResult.NoResult());}
            if (!ApiKeys.TryGetValue(providedApiKey, out var role)) {return Task.FromResult(AuthenticateResult.Fail("Invalid API Key"));}

            // ==== Start of Function ====
            // Create claims for user
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, role),
                new Claim(ClaimTypes.Role, role)
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
