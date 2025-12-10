// See https://aka.ms/new-console-template for more information
// ======== Imports ========
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using LeMarconnes.Shared.DTOs;

// ======== Namespace ========
namespace LeMarconnes.Client;

// Console client for the Gîte system. UI-only, logic lives in API.
class Program
{
    // ==== Properties ====
    // HttpClient voor alle API communicatie (static zodat hij hergebruikt wordt)
    private static readonly HttpClient _httpClient = new HttpClient();
    
    // Base URL van de API - pas aan als de API op een andere poort draait
    private static readonly string _baseUrl = "https://localhost:7221/api/gite";
    
    // JSON opties voor serialisatie/deserialisatie
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,  // "naam" en "Naam" worden beide herkend
        WriteIndented = true                  // Mooie formatting voor debug output
    };

    // ============================================================
    // ==== MAIN ENTRY POINT ====
    // ============================================================

    // Hoofdmethode van de applicatie - main loop en menu
    static async Task Main(string[] args)
    {
        // ==== Start of Function ====
        // UTF8 encoding voor speciale karakters (€ teken etc.)
        Console.OutputEncoding = Encoding.UTF8;
        PrintHeader();

        // Main loop - draait tot gebruiker kiest voor afsluiten (0)
        while (true)
        {
            try
            {
                PrintMainMenu();
                var choice = Console.ReadLine()?.Trim();

                // Switch op basis van gebruikerskeuze
                switch (choice)
                {
                    case "1": await BekijkEenhedenAsync(); break;
                    case "2": await CheckBeschikbaarheidAsync(); break;
                    case "3": await MaakBoekingAsync(); break;
                    case "4": await BekijkReserveringenAsync(); break;
                    case "5": await BekijkGastenAsync(); break;
                    case "6": await BekijkTarievenAsync(); break;
                    case "7": await BekijkPlatformenAsync(); break;
                    case "8": await AnnuleerReserveringAsync(); break;
                    case "9": await BekijkLogsAsync(); break;
                    case "0":
                        Console.WriteLine("\nTot ziens!");
                        return;  // Exit de applicatie
                    default:
                        Console.WriteLine("\nOngeldige keuze. Probeer opnieuw.");
                        break;
                }
            }
            catch (HttpRequestException ex)
            {
                // Specifieke foutafhandeling voor API verbindingsproblemen
                Console.WriteLine($"\nVerbindingsfout: Kan geen verbinding maken met de API.");
                Console.WriteLine($"   Zorg dat de API draait op {_baseUrl}");
                Console.WriteLine($"   Details: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Algemene foutafhandeling voor onverwachte fouten
                Console.WriteLine($"\nFout: {ex.Message}");
            }

            // Wacht op toetsdruk voordat scherm wordt gewist
            Console.WriteLine("\nDruk op een toets om door te gaan...");
            Console.ReadKey();
            Console.Clear();
            PrintHeader();
        }
    }

    // ============================================================
    // ==== INPUT HELPER METHODS ====
    // Deze methods vragen input met automatische retry bij fouten
    // ============================================================

    // Vraagt een datum aan de gebruiker, blijft vragen tot valide
    private static DateTime VraagDatum(string prompt)
    {
        // ==== Start of Function ====
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();
            
            // TryParse probeert de string om te zetten naar DateTime
            // Returns true als het lukt, false als het niet lukt
            if (DateTime.TryParse(input, out var datum))
                return datum;
            
            // Foutmelding in rood
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("   Ongeldige datum. Gebruik formaat: yyyy-MM-dd (bijv. 2025-07-01)");
            Console.ResetColor();
        }
    }

    // Vraagt een integer met optionele min/max validatie
    private static int VraagInteger(string prompt, int? min = null, int? max = null)
    {
        // ==== Start of Function ====
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();
            
            if (int.TryParse(input, out var waarde))
            {
                // Check minimum grens
                if (min.HasValue && waarde < min.Value)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"   Waarde moet minimaal {min.Value} zijn.");
                    Console.ResetColor();
                    continue;  // Opnieuw vragen
                }
                // Check maximum grens
                if (max.HasValue && waarde > max.Value)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"   Waarde mag maximaal {max.Value} zijn.");
                    Console.ResetColor();
                    continue;  // Opnieuw vragen
                }
                return waarde;
            }
            
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("   Ongeldige invoer. Voer een geldig getal in.");
            Console.ResetColor();
        }
    }

    // Vraagt een niet-lege string of default
    private static string VraagString(string prompt, bool verplicht = true, string? defaultWaarde = null)
    {
        // ==== Start of Function ====
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();
            
            // Als er iets is ingevoerd, return het
            if (!string.IsNullOrWhiteSpace(input))
                return input;
            
            // Als het veld niet verplicht is, return de default waarde
            if (!verplicht)
                return defaultWaarde ?? "";
            
            // Veld is verplicht maar leeg
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("   Dit veld is verplicht. Probeer opnieuw.");
            Console.ResetColor();
        }
    }

    // Optionele string, kan null zijn
    private static string? VraagOptional(string prompt)
    {
        // ==== Start of Function ====
        Console.Write(prompt);
        var input = Console.ReadLine();
        return string.IsNullOrWhiteSpace(input) ? null : input;
    }

    // Vraag ja/nee bevestiging, accepteert j/ja/n/nee
    private static bool VraagBevestiging(string prompt)
    {
        // ==== Start of Function ====
        while (true)
        {
            Console.Write($"{prompt} (j/n): ");
            var input = Console.ReadLine()?.ToLower().Trim();
            
            if (input == "j" || input == "ja") return true;
            if (input == "n" || input == "nee") return false;
            
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("   Voer 'j' voor ja of 'n' voor nee in.");
            Console.ResetColor();
        }
    }

    // Laat gebruiker kiezen uit set van geldige IDs
    private static int VraagKeuzeUitLijst(string prompt, HashSet<int> geldigeIds)
    {
        // ==== Start of Function ====
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();
            
            // Check of input een geldig getal is EN in de lijst voorkomt
            if (int.TryParse(input, out var id) && geldigeIds.Contains(id))
                return id;
            
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"   Ongeldige keuze. Kies uit: {string.Join(", ", geldigeIds.OrderBy(x => x))}");
            Console.ResetColor();
        }
    }

    // ============================================================
    // ==== UI METHODS ====
    // Methods voor het tonen van de user interface
    // ============================================================

    // Print de applicatie header/banner
    private static void PrintHeader()
    {
        // ==== Start of Function ====
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("================================================================");
        Console.WriteLine("           LE MARCONNES - GITE BEHEERSYSTEEM                   ");
        Console.WriteLine("                  Console Client v1.0                          ");
        Console.WriteLine("================================================================");
        Console.ResetColor();  // Reset naar standaard console kleur
    }

    // Print hoofdmenu
    private static void PrintMainMenu()
    {
        // ==== Start of Function ====
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n HOOFDMENU");
        Console.WriteLine("---------------------------------");
        Console.ResetColor();
        Console.WriteLine("  1. Bekijk alle eenheden");
        Console.WriteLine("  2. Check beschikbaarheid");
        Console.WriteLine("  3. Maak nieuwe boeking");
        Console.WriteLine("  4. Bekijk reserveringen");
        Console.WriteLine("  5. Bekijk gasten");
        Console.WriteLine("  6. Bekijk tarieven");
        Console.WriteLine("  7. Bekijk platformen");
        Console.WriteLine("  8. Annuleer reservering");
        Console.WriteLine("  9. Bekijk logs");
        Console.WriteLine("  0. Afsluiten");
        Console.WriteLine("---------------------------------");
        Console.Write("Keuze: ");
    }

    // Print sectie header
    private static void PrintSectionHeader(string title)
    {
        // ==== Start of Function ====
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n==================================================");
        Console.WriteLine($"  {title}");
        Console.WriteLine($"==================================================");
        Console.ResetColor();
    }

    // ============================================================
    // ==== API METHODS ====
    // Methods die communiceren met de Web API
    // ============================================================

    /// <summary>
    /// Haalt alle Gîte eenheden op van de API en toont ze in een tabel.
    /// Endpoint: GET /api/gite/eenheden
    /// </summary>
    private static async Task BekijkEenhedenAsync()
    {
        // ==== Start of Function ====
        PrintSectionHeader("ALLE GITE EENHEDEN");
        
        // HTTP GET request naar de API
        var response = await _httpClient.GetAsync($"{_baseUrl}/eenheden");
        response.EnsureSuccessStatusCode();  // Gooit exception als status niet 2xx is
        
        // JSON response omzetten naar List<VerhuurEenheidDTO>
        var units = await response.Content.ReadFromJsonAsync<List<VerhuurEenheidDTO>>(_jsonOptions);

        // Null/empty check
        if (units == null || units.Count == 0) 
        { 
            Console.WriteLine("Geen eenheden gevonden."); 
            return; 
        }

        // Tabel header
        Console.WriteLine($"\n{"ID",-5} {"Naam",-35} {"Type",-5} {"Cap",-5} {"Parent",-8}");
        Console.WriteLine(new string('-', 60));
        
        // Elke eenheid op een rij
        foreach (var unit in units)
        {
            var parentStr = unit.ParentEenheidID?.ToString() ?? "ROOT";  // null-coalescing voor parent
            var typeStr = unit.TypeID == 1 ? "Geh" : "Bed";  // 1=Geheel, 2=Slaapplek
            Console.WriteLine($"{unit.EenheidID,-5} {unit.Naam,-35} {typeStr,-5} {unit.MaxCapaciteit,-5} {parentStr,-8}");
        }
        Console.WriteLine($"\nTotaal: {units.Count} eenheden");
    }

    /// <summary>
    /// Checkt de beschikbaarheid van alle eenheden voor een opgegeven periode.
    /// Implementeert de Parent-Child blokkade logica:
    /// - Als Parent geboekt: Alle Children zijn BEZET
    /// - Als een Child geboekt: Parent is BEZET
    /// Endpoint: GET /api/gite/beschikbaarheid?startDatum=...&eindDatum=...
    /// </summary>
    private static async Task CheckBeschikbaarheidAsync()
    {
        // ==== Start of Function ====
        PrintSectionHeader("BESCHIKBAARHEID CHECKEN");
        
        // Input vragen met retry bij fouten
        var startDatum = VraagDatum("\nStartdatum (yyyy-MM-dd): ");
        var eindDatum = VraagDatum("Einddatum (yyyy-MM-dd): ");

        // Valideer dat einddatum na startdatum ligt
        while (eindDatum <= startDatum)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("   Einddatum moet NA de startdatum liggen.");
            Console.ResetColor();
            eindDatum = VraagDatum("Einddatum (yyyy-MM-dd): ");
        }

        // API call met query parameters
        var url = $"{_baseUrl}/beschikbaarheid?startDatum={startDatum:yyyy-MM-dd}&eindDatum={eindDatum:yyyy-MM-dd}";
        var response = await _httpClient.GetAsync(url);
        
        if (!response.IsSuccessStatusCode) 
        { 
            Console.WriteLine($"Fout: {await response.Content.ReadAsStringAsync()}"); 
            return; 
        }

        var units = await response.Content.ReadFromJsonAsync<List<VerhuurEenheidDTO>>(_jsonOptions);
        
        // Resultaat tonen
        Console.WriteLine($"\nBeschikbaarheid van {startDatum:dd-MM-yyyy} tot {eindDatum:dd-MM-yyyy}:\n");
        Console.WriteLine($"{"ID",-5} {"Naam",-35} {"Status",-12}");
        Console.WriteLine(new string('-', 55));

        foreach (var unit in units!)
        {
            var status = unit.IsBeschikbaar ? "VRIJ" : "BEZET";
            // Kleur op basis van beschikbaarheid
            Console.ForegroundColor = unit.IsBeschikbaar ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine($"{unit.EenheidID,-5} {unit.Naam,-35} {status}");
            Console.ResetColor();
        }
    }

    /// <summary>
    /// Begeleidt de gebruiker door het volledige boekingsproces:
    /// 1. Periode kiezen
    /// 2. Eenheid kiezen (uit beschikbare)
    /// 3. Platform kiezen
    /// 4. Gastgegevens invullen
    /// 5. Bevestigen en boeken
    /// Endpoint: POST /api/gite/boek
    /// </summary>
    private static async Task MaakBoekingAsync()
    {
        // ==== Declaring Variables ====
        DateTime startDatum, eindDatum;
        int eenheidId, platformId, aantalPersonen;
        string naam, email, straat, huisnr, postcode, plaats, land;
        string? tel;

        // ==== Start of Function ====
        PrintSectionHeader("NIEUWE BOEKING MAKEN");
        
        // ---- Stap 1: Periode kiezen ----
        Console.WriteLine("\n[Stap 1/4] Kies periode");
        startDatum = VraagDatum("Startdatum (yyyy-MM-dd): ");
        eindDatum = VraagDatum("Einddatum (yyyy-MM-dd): ");

        // Validatie: einddatum moet na startdatum
        while (eindDatum <= startDatum)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("   Einddatum moet NA de startdatum liggen.");
            Console.ResetColor();
            eindDatum = VraagDatum("Einddatum (yyyy-MM-dd): ");
        }

        // Beschikbaarheid ophalen van API
        var beschikUrl = $"{_baseUrl}/beschikbaarheid?startDatum={startDatum:yyyy-MM-dd}&eindDatum={eindDatum:yyyy-MM-dd}";
        var units = await (await _httpClient.GetAsync(beschikUrl)).Content.ReadFromJsonAsync<List<VerhuurEenheidDTO>>(_jsonOptions);
        
        // Filter op beschikbare eenheden
        var beschikbareUnits = units!.Where(u => u.IsBeschikbaar).ToList();
        if (beschikbareUnits.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\nGeen eenheden beschikbaar in deze periode!");
            Console.ResetColor();
            return;
        }

        // Toon beschikbare eenheden
        Console.WriteLine("\nBeschikbare eenheden:");
        foreach (var unit in beschikbareUnits)
            Console.WriteLine($"  [{unit.EenheidID}] {unit.Naam} (max {unit.MaxCapaciteit} pers)");

        // ---- Stap 2: Eenheid kiezen ----
        Console.WriteLine("\n[Stap 2/4] Kies eenheid");
        var geldigeEenheidIds = beschikbareUnits.Select(u => u.EenheidID).ToHashSet();
        eenheidId = VraagKeuzeUitLijst("EenheidID: ", geldigeEenheidIds);
        var gekozenEenheid = beschikbareUnits.First(u => u.EenheidID == eenheidId);

        // ---- Stap 3: Platform kiezen ----
        Console.WriteLine("\n[Stap 3/4] Kies platform");
        var platforms = await (await _httpClient.GetAsync($"{_baseUrl}/platformen")).Content.ReadFromJsonAsync<List<PlatformDTO>>(_jsonOptions);
        foreach (var p in platforms!) 
            Console.WriteLine($"  [{p.PlatformID}] {p.Naam} ({p.CommissiePercentage}% commissie)");
        
        var geldigePlatformIds = platforms.Select(p => p.PlatformID).ToHashSet();
        platformId = VraagKeuzeUitLijst("PlatformID: ", geldigePlatformIds);

        // ---- Stap 4: Gastgegevens ----
        Console.WriteLine("\n[Stap 4/4] Gastgegevens");
        naam = VraagString("Naam: ");
        email = VraagString("Email: ");
        tel = VraagOptional("Telefoon (optioneel): ");
        straat = VraagString("Straat: ");
        huisnr = VraagString("Huisnummer: ");
        postcode = VraagString("Postcode: ");
        plaats = VraagString("Plaats: ");
        land = VraagString("Land (default: Nederland): ", verplicht: false, defaultWaarde: "Nederland");
        aantalPersonen = VraagInteger("Aantal personen: ", min: 1, max: gekozenEenheid.MaxCapaciteit);

        // ---- Bevestiging ----
        Console.WriteLine("\n--- SAMENVATTING ---");
        Console.WriteLine($"Eenheid: {gekozenEenheid.Naam}");
        Console.WriteLine($"Periode: {startDatum:dd-MM-yyyy} t/m {eindDatum:dd-MM-yyyy} ({(eindDatum - startDatum).Days} nachten)");
        Console.WriteLine($"Gast: {naam} ({email})");
        Console.WriteLine($"Personen: {aantalPersonen}");

        if (!VraagBevestiging("\nBoeking bevestigen?"))
        {
            Console.WriteLine("Boeking geannuleerd.");
            return;
        }

        // ---- Boeking versturen naar API ----
        var boeking = new BoekingRequestDTO
        {
            GastNaam = naam, 
            GastEmail = email, 
            GastTel = tel, 
            GastStraat = straat,
            GastHuisnr = huisnr, 
            GastPostcode = postcode, 
            GastPlaats = plaats, 
            GastLand = land,
            EenheidID = eenheidId, 
            PlatformID = platformId, 
            StartDatum = startDatum, 
            EindDatum = eindDatum, 
            AantalPersonen = aantalPersonen
        };

        Console.WriteLine("\nBoeking wordt verwerkt...");
        var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/boek", boeking);
        var result = await response.Content.ReadFromJsonAsync<BoekingResponseDTO>(_jsonOptions);

        // Resultaat tonen
        if (result?.Succes == true)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n*** BOEKING SUCCESVOL ***");
            Console.WriteLine($"Reserveringsnummer: {result.ReserveringID}");
            Console.WriteLine($"Eenheid: {result.EenheidNaam}");
            Console.WriteLine($"Periode: {result.StartDatum:dd-MM-yyyy} t/m {result.EindDatum:dd-MM-yyyy}");
            Console.WriteLine($"Totaalprijs: EUR {result.TotaalPrijs:N2}");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nBoeking mislukt: {result?.FoutMelding}");
        }
        Console.ResetColor();
    }

    /// <summary>
    /// Haalt alle reserveringen op en toont ze in een tabel met kleurcodering.
    /// Kleurcodes: Geel=Gereserveerd, Groen=Ingecheckt, Rood=Geannuleerd
    /// Endpoint: GET /api/gite/reserveringen
    /// </summary>
    private static async Task BekijkReserveringenAsync()
    {
        // ==== Start of Function ====
        PrintSectionHeader("ALLE RESERVERINGEN");
        
        var reserveringen = await (await _httpClient.GetAsync($"{_baseUrl}/reserveringen"))
            .Content.ReadFromJsonAsync<List<ReserveringDTO>>(_jsonOptions);
        
        if (reserveringen == null || reserveringen.Count == 0) 
        { 
            Console.WriteLine("Geen reserveringen gevonden."); 
            return; 
        }

        // Tabel header
        Console.WriteLine($"\n{"ID",-5} {"Gast",-6} {"Eenh",-6} {"Start",-12} {"Eind",-12} {"Status",-15}");
        Console.WriteLine(new string('-', 60));
        
        foreach (var res in reserveringen)
        {
            // Kleur op basis van status
            Console.ForegroundColor = res.Status switch 
            { 
                "Gereserveerd" => ConsoleColor.Yellow, 
                "Ingecheckt" => ConsoleColor.Green, 
                "Geannuleerd" => ConsoleColor.Red, 
                _ => ConsoleColor.White 
            };
            Console.WriteLine($"{res.ReserveringID,-5} {res.GastID,-6} {res.EenheidID,-6} {res.Startdatum:dd-MM-yyyy}  {res.Einddatum:dd-MM-yyyy}  {res.Status}");
            Console.ResetColor();
        }
        Console.WriteLine($"\nTotaal: {reserveringen.Count} reserveringen");
    }

    /// <summary>
    /// Haalt alle gasten op en toont ze in een tabel.
    /// Endpoint: GET /api/gite/gasten
    /// </summary>
    private static async Task BekijkGastenAsync()
    {
        // ==== Start of Function ====
        PrintSectionHeader("ALLE GASTEN");
        
        var gasten = await (await _httpClient.GetAsync($"{_baseUrl}/gasten"))
            .Content.ReadFromJsonAsync<List<GastDTO>>(_jsonOptions);
        
        if (gasten == null || gasten.Count == 0) 
        { 
            Console.WriteLine("Geen gasten gevonden."); 
            return; 
        }

        Console.WriteLine($"\n{"ID",-5} {"Naam",-25} {"Email",-30} {"Plaats",-15}");
        Console.WriteLine(new string('-', 80));
        
        foreach (var gast in gasten) 
            Console.WriteLine($"{gast.GastID,-5} {gast.Naam,-25} {gast.Email,-30} {gast.Plaats,-15}");
        
        Console.WriteLine($"\nTotaal: {gasten.Count} gasten");
    }

    /// <summary>
    /// Haalt alle tarieven op en toont ze in een tabel.
    /// Endpoint: GET /api/gite/tarieven
    /// </summary>
    private static async Task BekijkTarievenAsync()
    {
        // ==== Start of Function ====
        PrintSectionHeader("ALLE TARIEVEN");
        
        var tarieven = await (await _httpClient.GetAsync($"{_baseUrl}/tarieven"))
            .Content.ReadFromJsonAsync<List<TariefDTO>>(_jsonOptions);
        
        if (tarieven == null || tarieven.Count == 0) 
        { 
            Console.WriteLine("Geen tarieven gevonden."); 
            return; 
        }

        Console.WriteLine($"\n{"ID",-5} {"Type",-6} {"Platform",-10} {"Prijs",-12} {"Tax",-6} {"Geldig vanaf",-15}");
        Console.WriteLine(new string('-', 60));
        
        foreach (var t in tarieven) 
            Console.WriteLine($"{t.TariefID,-5} {t.TypeID,-6} {t.PlatformID?.ToString() ?? "-",-10} EUR {t.Prijs,-8:N2} {(t.TaxStatus ? "Incl" : "Excl"),-6} {t.GeldigVan:dd-MM-yyyy}");
    }

    /// <summary>
    /// Haalt alle platformen op en toont ze in een tabel.
    /// Endpoint: GET /api/gite/platformen
    /// </summary>
    private static async Task BekijkPlatformenAsync()
    {
        // ==== Start of Function ====
        PrintSectionHeader("ALLE PLATFORMEN");
        
        var platforms = await (await _httpClient.GetAsync($"{_baseUrl}/platformen"))
            .Content.ReadFromJsonAsync<List<PlatformDTO>>(_jsonOptions);
        
        if (platforms == null || platforms.Count == 0) 
        { 
            Console.WriteLine("Geen platformen gevonden."); 
            return; 
        }

        Console.WriteLine($"\n{"ID",-5} {"Naam",-25} {"Commissie",-15}");
        Console.WriteLine(new string('-', 50));
        
        foreach (var p in platforms) 
            Console.WriteLine($"{p.PlatformID,-5} {p.Naam,-25} {p.CommissiePercentage}%");
    }

    /// <summary>
    /// Annuleert een reservering na bevestiging.
    /// Toont eerst alle actieve reserveringen, laat gebruiker kiezen.
    /// Endpoint: PUT /api/gite/reserveringen/{id}/annuleer
    /// </summary>
    private static async Task AnnuleerReserveringAsync()
    {
        // ==== Start of Function ====
        PrintSectionHeader("RESERVERING ANNULEREN");
        
        // Haal alle reserveringen op
        var reserveringen = await (await _httpClient.GetAsync($"{_baseUrl}/reserveringen"))
            .Content.ReadFromJsonAsync<List<ReserveringDTO>>(_jsonOptions);
        
        if (reserveringen == null || reserveringen.Count == 0) 
        { 
            Console.WriteLine("Geen reserveringen gevonden."); 
            return; 
        }

        // Filter: toon alleen actieve reserveringen (niet al geannuleerd)
        var actieveReserveringen = reserveringen.Where(r => r.Status != "Geannuleerd").ToList();
        if (actieveReserveringen.Count == 0)
        {
            Console.WriteLine("Geen actieve reserveringen om te annuleren.");
            return;
        }

        // Toon actieve reserveringen
        Console.WriteLine($"\n{"ID",-5} {"Gast",-6} {"Eenh",-6} {"Start",-12} {"Eind",-12} {"Status",-15}");
        Console.WriteLine(new string('-', 60));
        foreach (var res in actieveReserveringen)
        {
            Console.WriteLine($"{res.ReserveringID,-5} {res.GastID,-6} {res.EenheidID,-6} {res.Startdatum:dd-MM-yyyy}  {res.Einddatum:dd-MM-yyyy}  {res.Status}");
        }

        // Laat gebruiker kiezen uit geldige IDs
        var geldigeIds = actieveReserveringen.Select(r => r.ReserveringID).ToHashSet();
        var id = VraagKeuzeUitLijst("\nReserveringID om te annuleren: ", geldigeIds);

        // Vraag bevestiging
        if (!VraagBevestiging($"Weet je zeker dat je reservering {id} wilt annuleren?"))
        {
            Console.WriteLine("Annulering geannuleerd.");
            return;
        }

        // Verstuur PUT request naar API
        var response = await _httpClient.PutAsync($"{_baseUrl}/reserveringen/{id}/annuleer", null);
        
        Console.ForegroundColor = response.IsSuccessStatusCode ? ConsoleColor.Green : ConsoleColor.Red;
        Console.WriteLine(response.IsSuccessStatusCode 
            ? $"\nReservering {id} is geannuleerd." 
            : $"\nFout: {await response.Content.ReadAsStringAsync()}");
        Console.ResetColor();
    }

    /// <summary>
    /// Haalt de meest recente log entries (audit trail) op.
    /// Endpoint: GET /api/gite/logs?count=20
    /// </summary>
    private static async Task BekijkLogsAsync()
    {
        // ==== Start of Function ====
        PrintSectionHeader("RECENTE LOGS (Audit Trail)");
        
        var logs = await (await _httpClient.GetAsync($"{_baseUrl}/logs?count=20"))
            .Content.ReadFromJsonAsync<List<LogboekDTO>>(_jsonOptions);
        
        if (logs == null || logs.Count == 0) 
        { 
            Console.WriteLine("Geen logs gevonden."); 
            return; 
        }

        Console.WriteLine($"\n{"Tijd",-20} {"Actie",-25} {"Tabel",-20} {"Record",-8}");
        Console.WriteLine(new string('-', 80));
        
        foreach (var log in logs) 
            Console.WriteLine($"{log.Tijdstip:dd-MM-yyyy HH:mm}  {log.Actie,-25} {log.TabelNaam ?? "-",-20} {log.RecordID?.ToString() ?? "-",-8}");
    }
}
