// ======== Imports ========
using LeMarconnes.API.DAL.Interfaces;
using LeMarconnes.API.DAL.Repositories;

// ============================================================
// ======== BUILDER CONFIGURATION ========
// Dit is het entry point van de ASP.NET Core Web API.
// Hier worden alle services geconfigureerd (Dependency Injection).
// ============================================================

var builder = WebApplication.CreateBuilder(args);

// ==== Add services to the container ====

// Controllers - activeert de MVC/API controller functionaliteit
builder.Services.AddControllers();

// OpenAPI / Swagger - voor API documentatie en testing
// Swagger UI is beschikbaar op /swagger in development mode
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DAL - Repository (Dependency Injection)
// AddScoped = nieuwe instantie per HTTP request
// Dit zorgt ervoor dat elke request zijn eigen repository heeft,
// maar dezelfde repository wordt hergebruikt binnen één request.
// 
// Interface -> Implementatie mapping:
// Wanneer code IGiteRepository vraagt, krijgt het GiteRepository
builder.Services.AddScoped<IGiteRepository, GiteRepository>();

// ============================================================
// ======== APP CONFIGURATION ========
// Hier wordt de HTTP pipeline geconfigureerd.
// De volgorde van middleware is belangrijk!
// ============================================================

var app = builder.Build();

// Development-only middleware
if (app.Environment.IsDevelopment()) 
{
    // Swagger UI beschikbaar maken voor API testing
    // Bereikbaar via: https://localhost:7221/swagger
    app.UseSwagger();
    app.UseSwaggerUI();
}

// HTTPS Redirection - redirect HTTP requests naar HTTPS
app.UseHttpsRedirection();

// Authorization middleware - voor toekomstige authenticatie/autorisatie
app.UseAuthorization();

// Map controllers - koppelt de API endpoints aan de controllers
// Routes worden bepaald door [Route] en [Http*] attributen op de controllers
app.MapControllers();

// ============================================================
// ======== START APPLICATION ========
// Start de Kestrel webserver en begin met luisteren naar requests.
// Blokkeert tot de applicatie wordt afgesloten.
// ============================================================

app.Run();
