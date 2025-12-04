using ConsoleApp1.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

public class Waarneming : API {
    // ==== Properties ====
    [JsonPropertyName("id")]
    public int WaarnemingID { get; set; }

    [JsonPropertyName("soortId")]
    public int SoortID { get; set; }

    [JsonPropertyName("datum")]
    public DateTime Datum { get; set; }

    [JsonPropertyName("tijd")]
    public TimeSpan Tijd { get; set; }

    [JsonPropertyName("aantalIndividuen")]
    public int AantalIndividuen { get; set; }

    [JsonPropertyName("geslacht")]
    public string Geslacht { get; set; }

    [JsonPropertyName("isGevalideerd")]
    public bool IsGevalideerd { get; set; }

    [JsonPropertyName("waarnemingLinks")]
    public string WaarnemingLinks { get; set; }

    // ==== Methods ====
    public void ToonWaarnemingDetails() {
        Console.WriteLine($"Soort ID: {this.SoortID} | Datum: {this.Datum} | Tijd: {this.Tijd} | Is gevalideerd: {this.IsGevalideerd}");
        Console.WriteLine($"Geslacht: {this.Geslacht} | Aantal individuen: {this.AantalIndividuen}");
        Console.WriteLine($"Waarneming links: {this.WaarnemingLinks}\n");
    }

    public async Task<List<Waarneming>> HaalWaarnemingenOp() {
        var data = await this.VerkrijgDataVanAPI(this.VerkrijgAPIURL());

        if (data != null) {
            List<Waarneming> waarnemingen = JsonSerializer.Deserialize<List<Waarneming>>(data);
            return waarnemingen;
        }
        return new List<Waarneming>();
    }

    public async Task PostWaarneming(Waarneming waarneming) {
        // Making it into a good json
        string jsonString = JsonSerializer.Serialize(waarneming);

        bool requestResultaat = await this.PostDataNaarAPI(jsonString);

        if (requestResultaat) {
            Console.WriteLine("Waarneming toegevoegd.");
        }
        else {
            Console.WriteLine("Er is iets fout gegaan bij het toevoegen van de waarneming.");
        }
    }

    public async Task UpdateWaarneming(Waarneming waarneming) {
        // Checking if the soortID exists
        

        // Making it into a good json
        string jsonString = JsonSerializer.Serialize(waarneming);
        bool requestResultaat = await this.PUTDataNaarAPI(waarneming.WaarnemingID.ToString(), jsonString);

        if (requestResultaat) {
            Console.WriteLine("Waarneming bijgewerkt.");
        }
        else {
            Console.WriteLine("Er is iets fout gegaan bij het bijwerken van de waarneming.");
        }
    }

    public async Task DeleteWaarneming(int waarnemingID) {
        bool requestResultaat = await this.DeleteNaarAPI(waarnemingID.ToString());

        if (requestResultaat) {
            Console.WriteLine("Waarneming verwijderd.\n");
        }
        else {
            Console.WriteLine("Er is iets fout gegaan bij het verwijderen van de waarneming.");
        }
    }

    public async Task<Waarneming> WaarnemingPrompt() {
        // ==== Declaring Variables ====
        int soortID, aantalIndividuen;
        string geslacht;
        bool isGevalideerd;
        TimeSpan tijd;
        DateTime datum;
        Soort soortInstance = new Soort();

        while (true) {
            Console.WriteLine("SoortID:");
            string invoer = Console.ReadLine();
            if (int.TryParse(invoer, out soortID)) {
                // Check if the SoortID exists
                bool soortBestaan = await soortInstance.HaalSoortOp(soortID);
                if (soortBestaan) {
                    break;
                }
                Console.WriteLine("\nSoortID bestaat niet.\n");
            }
            else {
                Console.WriteLine("Ongeldige invoer. Voer een getal in.");
            }
        }

        while (true) {
            Console.WriteLine("Tijd:");
            string invoer = Console.ReadLine();
            if (TimeSpan.TryParse(invoer, out tijd)) { break; }
            Console.WriteLine("Ongeldige invoer. Voer een tijd in.");
        }

        while (true) {
            Console.WriteLine("Datum:");
            string invoer = Console.ReadLine();
            if (DateTime.TryParse(invoer, out datum)) { break; }
            Console.WriteLine("Ongeldige invoer. Voer een datum in.");
        }

        while (true) {
            Console.WriteLine("Aantal individuen:");
            string invoer = Console.ReadLine();
            if (int.TryParse(invoer, out aantalIndividuen)) { break; }
            Console.WriteLine("Ongeldige invoer. Voer een getal in.");
        }

        while (true) {
            Console.WriteLine("Geslacht:");
            geslacht = Console.ReadLine();

            if (geslacht != null) { break; }
            Console.WriteLine("Ongeldige invoer. Voer een geslacht in.");
        }

        while (true) {
            Console.WriteLine("Is de waarneming gevalideerd (ja/nee)?:");
            string invoer = Console.ReadLine();
            if (invoer is "ja" or "nee") { isGevalideerd = invoer == "ja" ? true : false; break; }
            Console.WriteLine("Ongeldige invoer. Voer ja of nee in.");
        }

        // ==== Start of Function ====
        Waarneming nieuweWaarneming = new Waarneming {
            SoortID = soortID,
            Tijd = tijd,
            Datum = datum,
            AantalIndividuen = aantalIndividuen,
            Geslacht = geslacht,
            IsGevalideerd = isGevalideerd,
            WaarnemingLinks = ""
        };
        return nieuweWaarneming;
    }

    // ==== Constructor ====
    public Waarneming() : base(apiURL: "http://48.209.56.82:5001/api/Waarneming") {
        this.WaarnemingID = 0;
        this.SoortID = 0;
        this.Datum = new DateTime();
        this.Tijd = new TimeSpan();
        this.AantalIndividuen = 0;
        this.Geslacht = "";
        this.IsGevalideerd = false;
        this.WaarnemingLinks = "";
    }

    public Waarneming(int waarnemingID, int soortID, DateTime datum, TimeSpan tijd, int aantalIndividuen, string geslacht, bool isGevalideerd, string waarnemingLinks) : base(apiURL: "http://48.209.56.82:5001/api/Waarneming") {
        this.WaarnemingID = waarnemingID;
        this.SoortID = soortID;
        this.Datum = datum;
        this.Tijd = tijd;
        this.AantalIndividuen = aantalIndividuen;
        this.Geslacht = geslacht;
        this.IsGevalideerd = isGevalideerd;
        this.WaarnemingLinks = waarnemingLinks;
    }
}