using System.Text.Json;
using System.Text.Json.Serialization;

namespace ConsoleApp1.Models;

public class Soort : API {
    // ==== Properties ====
    [JsonPropertyName("id")]
    public int SoortID { get; set; }

    [JsonPropertyName("naam")]
    public string Naam { get; set; }

    [JsonPropertyName("latijnseNaam")]
    public string Beschrijving { get; set; }

    [JsonPropertyName("zeldzaamheid")]
    public string Zeldzaamheid { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    // ==== Methods ====
    public void ToonSoortDetails() {
        Console.WriteLine($"Soort ID: {this.SoortID} | Naam: {this.Naam} | Latijnse Naam: {this.Beschrijving}");
        Console.WriteLine($"Zeldzaamheid: {this.Zeldzaamheid} | Status: {this.Status}\n");
    }

    public async Task<List<Soort>> HaalSoortenOp() {
        var data = await this.VerkrijgDataVanAPI(this.VerkrijgAPIURL());

        if (data != null) {
            List<Soort> soorten = JsonSerializer.Deserialize<List<Soort>>(data);
            return soorten;
        }
        return new List<Soort>();
    }

    public async Task<bool> HaalSoortOp(int soortID)
    {
        var data = await this.VerkrijgDataVanAPI(this.VerkrijgAPIURL(), soortID);

        if (data != null) {
            Soort soort = JsonSerializer.Deserialize<Soort>(data);
            if (soort != null) {
                return true;
            }
        }
        return false;
    }

    public async Task PostSoort(Soort soort) {
        string jsonString = JsonSerializer.Serialize(soort);

        bool requestResultaat = await this.PostDataNaarAPI(jsonString);

        if (requestResultaat) {
            Console.WriteLine("Soort toegevoegd.");
        }
        else {
            Console.WriteLine("Er is iets fout gegaan bij het toevoegen van de soort.");
        }
    }

    public async Task UpdateSoort(Soort soort) {
        string jsonString = JsonSerializer.Serialize(soort);

        bool requestResultaat = await this.PUTDataNaarAPI(soort.SoortID.ToString(), jsonString);

        if (requestResultaat) {
            Console.WriteLine("Soort bijgewerkt.");
        }
        else {
            Console.WriteLine("Er is iets fout gegaan bij het bijwerken van de soort.");
        }
    }

    public async Task DeleteSoort(int soortID) {
        bool requestResultaat = await this.DeleteNaarAPI(soortID.ToString());

        if (requestResultaat) {
            Console.WriteLine("Soort verwijderd.\n");
        }
        else {
            Console.WriteLine("Er is iets fout gegaan bij het verwijderen van de soort.");
        }
    }

    public Soort SoortPrompt() {
        // ==== Declaring Variables ====
        string naam, beschrijving, zeldzaamheid, status;

        Console.WriteLine("Naam:");
        naam = Console.ReadLine();

        Console.WriteLine("Latijnse Naam:");
        beschrijving = Console.ReadLine();

        Console.WriteLine("Zeldzaamheid:");
        zeldzaamheid = Console.ReadLine();

        Console.WriteLine("Status:");
        status = Console.ReadLine();

        // ==== Start of Function ====
        Soort nieuwSoort = new Soort {
            Naam = naam,
            Beschrijving = beschrijving,
            Zeldzaamheid = zeldzaamheid,
            Status = status
        };
        return nieuwSoort;
    }

    // ==== Constructor ====
    public Soort() : base(apiURL: "http://48.209.56.82:5001/api/Soorten") {
        this.SoortID = 0;
        this.Naam = "";
        this.Beschrijving = "";
        this.Zeldzaamheid = "";
        this.Status = "";
    }

    public Soort(int soortID, string naam, string beschrijving, string zeldzaamheid, string status)
        : base(apiURL: "http://48.209.56.82:5001/api/Soorten") {
        this.SoortID = soortID;
        this.Naam = naam;
        this.Beschrijving = beschrijving;
        this.Zeldzaamheid = zeldzaamheid;
        this.Status = status;
    }
}
