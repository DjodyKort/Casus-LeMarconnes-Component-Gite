using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Http;
using LeMarconnes.API.DAL.Interfaces;
using Moq;
using System.Security.Claims;

namespace LeMarconnes.API.Tests;

/// <summary>
/// Custom WebApplicationFactory voor integration tests met mock repository en test authentication.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    public Mock<IGiteRepository> MockRepository { get; } = new Mock<IGiteRepository>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Remove de echte repository
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IGiteRepository));

            if (descriptor != null)
                services.Remove(descriptor);

            // Voeg de mock repository toe
            services.AddScoped(_ => MockRepository.Object);

            // Add test authentication
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
        });

        builder.ConfigureServices(services =>
        {
            // Override authentication to use Test scheme
            services.Configure<Microsoft.AspNetCore.Authentication.AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            });
        });
    }
}

/// <summary>
/// Test authentication handler that always succeeds and adds Admin role.
/// </summary>
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(
        Microsoft.Extensions.Options.IOptionsMonitor<AuthenticationSchemeOptions> options,
        Microsoft.Extensions.Logging.ILoggerFactory logger,
        System.Text.Encodings.Web.UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Check if endpoint has [AllowAnonymous] attribute
        var endpoint = Context.GetEndpoint();
        if (endpoint?.Metadata?.GetMetadata<Microsoft.AspNetCore.Authorization.IAllowAnonymous>() != null)
        {
            // Don't authenticate for AllowAnonymous endpoints
            return AuthenticateResult.NoResult();
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "TestUser"),
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(ClaimTypes.Role, "User")
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return AuthenticateResult.Success(ticket);
    }
}
