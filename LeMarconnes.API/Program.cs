// ======== Imports ========
using LeMarconnes.API.DAL.Interfaces;
using LeMarconnes.API.DAL.Repositories;
using LeMarconnes.API.Services.Interfaces;
using LeMarconnes.API.Services.Implementations;
using LeMarconnes.API.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.HttpOverrides;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// 1. SERVICES & AUTH CONFIGURATION
// ============================================================

// CORS Toevoegen
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// API Key Authentication
builder.Services.AddAuthentication("ApiKey")
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>("ApiKey", null);

builder.Services.AddAuthorization();
builder.Services.AddControllers();

// ============================================================
// 2. OPENAPI / SCALAR SETUP
// ============================================================

builder.Services.AddOpenApi("v1", options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Tags?.Clear();

        document.Info = new OpenApiInfo
        {
            Title = "LeMarconnes Gîte API",
            Version = "v1",
            Description = """
                          API voor het beheren van verhuur eenheden, reserveringen en gasten.
                          
                          **Authenticatie met API Keys:**
                          - 🔓 **Public**: Geen key vereist (beschikbaarheid, lookups, tarieven, boeken)
                          - 👤 **User**: `user-key-67890` (eigen gasten/reserveringen beheren)
                          - 🔐 **Admin**: `admin-key-12345` (volledige toegang & logboeken)
                          
                          Klik op de **Authorize** knop (of 'Auth') om je API key in te voeren.
                          """
        };

        document.Servers = new List<OpenApiServer> { new() { Url = "/" } };

        var securityScheme = new OpenApiSecurityScheme
        {
            Name = "X-API-Key",
            Description = "Voer API Key in (bijv. 'admin-key-12345')",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey
        };

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes.Add("ApiKey", securityScheme);

        document.SecurityRequirements.Add(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" }
                },
                Array.Empty<string>()
            }
        });

        return Task.CompletedTask;
    });

    options.AddOperationTransformer((operation, context, cancellationToken) =>
    {
        var metadata = context.Description.ActionDescriptor.EndpointMetadata;
        operation.Tags.Clear();

        if (metadata.OfType<AllowAnonymousAttribute>().Any())
        {
            operation.Tags.Add(new OpenApiTag { Name = "🔓 Public Actions" });
            operation.Security.Clear();
            return Task.CompletedTask;
        }

        var authAttribute = metadata.OfType<AuthorizeAttribute>().FirstOrDefault();
        if (authAttribute?.Roles != null)
        {
            var tag = authAttribute.Roles.Contains("Admin") && !authAttribute.Roles.Contains("User")
                ? "🔐 Admin Actions"
                : "👤 User Actions";
            operation.Tags.Add(new OpenApiTag { Name = tag });
        }
        else
        {
            operation.Tags.Add(new OpenApiTag { Name = "🔒 Secured Actions" });
        }

        return Task.CompletedTask;
    });
});

// ============================================================
// 3. DI (Repositories & Services)
// ============================================================

builder.Services.AddScoped<IGiteRepository, GiteRepository>();
builder.Services.AddScoped<IBeschikbaarheidService, BeschikbaarheidService>();
builder.Services.AddScoped<IBoekingService, BoekingService>();
builder.Services.AddScoped<IPrijsberekeningService, PrijsberekeningService>();
builder.Services.AddScoped<IWachtwoordService, WachtwoordService>();

// ============================================================
// 4. APP PIPELINE
// ============================================================

var app = builder.Build();

app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseCors("AllowAll");

// ============================================================
// OPENAPI DTO DUPLICATEN FIX
// ============================================================
// Fix DTO2, DTO3, etc. duplicaten die .NET OpenAPI generator maakt bij cyclische referenties
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/openapi/v1.json"))
    {
        var originalBody = context.Response.Body;
        using var newBody = new MemoryStream();
        context.Response.Body = newBody;
        
        await next();
        
        newBody.Seek(0, SeekOrigin.Begin);
        var json = await new StreamReader(newBody).ReadToEndAsync();
        
        // Simpele regex: "VerhuurEenheidDTO2" -> "VerhuurEenheidDTO", etc.
        json = Regex.Replace(json, @"(\w+DTO)\d+", "$1");
        
        context.Response.Body = originalBody;
        context.Response.ContentLength = System.Text.Encoding.UTF8.GetByteCount(json);
        await context.Response.WriteAsync(json);
        return;
    }
    
    await next();
});

app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.WithTitle("LeMarconnes API");
    options.WithTheme(ScalarTheme.Mars);
    options.WithPreferredScheme("ApiKey");
    options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }