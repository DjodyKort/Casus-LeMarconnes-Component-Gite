// ======== Imports ========
using LeMarconnes.API.DAL.Interfaces;
using LeMarconnes.API.DAL.Repositories;

// ============================================================
// BUILDER CONFIGURATION
// Entry point van de ASP.NET Core Web API. Services en middleware.
// ============================================================

var builder = WebApplication.CreateBuilder(args);

// ==== Add services to the container ====

// Controllers - activeert de MVC/API controller functionaliteit
builder.Services.AddControllers();

// OpenAPI / Swagger - voor API documentatie en testing
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DAL - Repository (Dependency Injection)
// AddScoped = nieuwe instantie per HTTP request
builder.Services.AddScoped<IGiteRepository, GiteRepository>();

// ============================================================
// APP CONFIGURATION
// Configureer de HTTP pipeline
// ============================================================

var app = builder.Build();

if (app.Environment.IsDevelopment()) 
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// ============================================================
// START APPLICATION
// Start de webserver
// ============================================================

app.Run();
