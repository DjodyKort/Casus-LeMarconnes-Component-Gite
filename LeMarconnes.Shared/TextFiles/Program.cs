// ======== Imports ========
using System.Text.Json;
using ConsoleApp1.Models;
using Microsoft.VisualBasic;
using static System.Runtime.InteropServices.JavaScript.JSType;

// ======== Namespace ========
namespace ConsoleApp1 {
    internal class Program {
        // ======== Functions ========
        private static async Task ToonWaarnemingen() {
            Waarneming waarnemingInstance = new Waarneming();
            List<Waarneming> waarnemingen = await waarnemingInstance.HaalWaarnemingenOp();

            Console.WriteLine("\nAlle waarnemingen:");
            foreach (Waarneming waarneming in waarnemingen) {
                waarneming.ToonWaarnemingDetails();
            }
        }

        private static async Task VoegWaarnemingToe() {
            // ==== Declaring Variables ====
            Waarneming waarnemingInstance = new Waarneming();
            Waarneming nieuwWaarneming = await waarnemingInstance.WaarnemingPrompt();

            // ==== Start of Function ====
            await waarnemingInstance.PostWaarneming(nieuwWaarneming);
        }

        private static async Task UpdateWaarneming() {
            // ==== Declaring Variables ====
            string invoer;
            int keuze;

            Waarneming waarnemingInstance = new Waarneming();
            Soort soortInstance = new Soort();
            List<Waarneming> waarnemingen = await waarnemingInstance.HaalWaarnemingenOp();

            // ==== Start of Function ====
            Console.WriteLine("\nWelke waarneming zou je willen aanpassen?");

            for (int i = 0; i < waarnemingen.Count; i++) {
                Console.WriteLine($"{i + 1}. Soort ID: {waarnemingen[i].SoortID}, Datum: {waarnemingen[i].Datum}");
            }

            while (true) {
                invoer = Console.ReadLine();

                if (int.TryParse(invoer, out keuze)) {
                    if (keuze >= 1 && keuze <= waarnemingen.Count) {
                        break;
                    }
                    Console.WriteLine($"Ongeldige keuze. Kies een nummer tussen 1 en {waarnemingen.Count}.");
                }
            }

            Waarneming waarneming = waarnemingen[keuze - 1];
            waarneming.ToonWaarnemingDetails();

            Console.WriteLine("\nWelk detail moet aangepast worden?\n1. SoortID\n2. Datum\n3. Tijd\n4. Aantal Individuen\n5. Geslacht\n6. Is Gevalideerd\n7. Waarneming Links");

            invoer = Console.ReadLine();
            switch (invoer) {
                case "1":
                    while (true) {
                        Console.WriteLine("Nieuwe SoortID:");
                        if (int.TryParse(Console.ReadLine(), out int nieuweSoortID)) {
                            waarneming.SoortID = nieuweSoortID;
                        }

                        // Check if the SoortID exists
                        bool soortBestaan = await soortInstance.HaalSoortOp(waarneming.SoortID);
                        if (soortBestaan) {
                            break;
                        }
                        Console.WriteLine("\nSoortID bestaat niet.\n");
                    }
                    break;
                case "2":
                    Console.WriteLine("Nieuwe Datum:");
                    if (DateTime.TryParse(Console.ReadLine(), out DateTime nieuweDatum)) {
                        waarneming.Datum = nieuweDatum;
                    }
                    break;
                case "3":
                    Console.WriteLine("Nieuwe Tijd:");
                    if (TimeSpan.TryParse(Console.ReadLine(), out TimeSpan nieweTijd)) {
                        waarneming.Tijd = nieweTijd;
                    }
                    break;
                case "4":
                    Console.WriteLine("Nieuw Aantal Individuen:");
                    if (int.TryParse(Console.ReadLine(), out int nieuwAantalIndividuen)) {
                        waarneming.AantalIndividuen = nieuwAantalIndividuen;
                    }
                    break;
                case "5":
                    Console.WriteLine("Nieuw Geslacht:");
                    waarneming.Geslacht = Console.ReadLine();
                    break;
                case "6":
                    Console.WriteLine("Is Gevalideerd (ja/nee):");
                    string isGevalideerdInput = Console.ReadLine();
                    waarneming.IsGevalideerd = isGevalideerdInput.ToLower() == "ja";
                    break;
                case "7":
                    Console.WriteLine("Nieuwe Waarneming Links:");
                    waarneming.WaarnemingLinks = Console.ReadLine();
                    break;
                default:
                    Console.WriteLine("Ongeldige keuze.");
                    break;
            }

            await waarnemingInstance.UpdateWaarneming(waarneming);
        }

        private static async Task DeleteWaarneming() {
            // ==== Declaring Variables ====
            string invoer;
            int keuze, waarnemingID;

            Waarneming waarnemingInstance = new Waarneming();
            List<Waarneming> waarnemingen = await waarnemingInstance.HaalWaarnemingenOp();

            // ==== Start of Function ====
            Console.WriteLine("\nWelke waarneming zou je willen verwijderen?");

            for (int i = 0; i < waarnemingen.Count; i++) {
                Console.WriteLine($"{i + 1}. Soort ID: {waarnemingen[i].SoortID}, Datum: {waarnemingen[i].Datum}");
            }

            while (true) {
                invoer = Console.ReadLine();

                if (int.TryParse(invoer, out keuze)) {
                    if (keuze >= 1 && keuze <= waarnemingen.Count) {
                        break;
                    }
                    Console.WriteLine($"Ongeldige keuze. Kies een nummer tussen 1 en {waarnemingen.Count}.");
                }
            }

            await waarnemingInstance.DeleteWaarneming(waarnemingen[keuze - 1].WaarnemingID);
        }

        private static async Task LaatSoortenZien() {
            // ==== Declaring Variables ====
            Soort soortInstance = new Soort();
            List<Soort> soorten = await soortInstance.HaalSoortenOp();

            // ==== Start of Function ====
            Console.WriteLine("\nAlle soorten:");
            foreach (Soort soort in soorten) {
                soort.ToonSoortDetails();
            }
        }

        private static async Task VoegSoortToe() {
            // ==== Declaring Variables ====
            Soort soortInstance = new Soort();
            Soort nieuweSoort = soortInstance.SoortPrompt();

            // ==== Start of Function ====
            await soortInstance.PostSoort(nieuweSoort);
        }

        private static async Task UpdateSoort() {
            // ==== Declaring Variables ====
            string invoer;
            int keuze;

            Soort soortInstance = new Soort();
            List<Soort> soorten = await soortInstance.HaalSoortenOp();

            // ==== Start of Function ====
            Console.WriteLine("\nWelke soort zou je willen aanpassen?");

            for (int i = 0; i < soorten.Count; i++) {
                Console.WriteLine($"{i + 1}. Soort ID: {soorten[i].SoortID}, Naam: {soorten[i].Naam}");
            }

            while (true) {
                invoer = Console.ReadLine();

                if (int.TryParse(invoer, out keuze)) {
                    if (keuze >= 1 && keuze <= soorten.Count) {
                        break;
                    }
                    Console.WriteLine($"Ongeldige keuze. Kies een nummer tussen 1 en {soorten.Count}.");
                }
            }

            Soort soort = soorten[keuze - 1];
            soort.ToonSoortDetails();

            Console.WriteLine("\nWelk detail moet aangepast worden?\n1. Naam\n2. Latijnse Naam\n3. Zeldzaamheid\n4. Status");

            invoer = Console.ReadLine();
            switch (invoer) {
                case "1":
                    Console.WriteLine("Nieuwe Naam:");
                    soort.Naam = Console.ReadLine();
                    break;
                case "2":
                    Console.WriteLine("Nieuwe Latijnse Naam:");
                    soort.Beschrijving = Console.ReadLine();
                    break;
                case "3":
                    Console.WriteLine("Nieuwe Zeldzaamheid:");
                    soort.Zeldzaamheid = Console.ReadLine();
                    break;
                case "4":
                    Console.WriteLine("Nieuwe Status:");
                    soort.Status = Console.ReadLine();
                    break;
                default:
                    Console.WriteLine("Ongeldige keuze.");
                    break;
            }

            await soortInstance.UpdateSoort(soort);
        }

        private static async Task DeleteSoort() {
            // ==== Declaring Variables ====
            string invoer;
            int keuze;

            Soort soortInstance = new Soort();
            List<Soort> soorten = await soortInstance.HaalSoortenOp();

            // ==== Start of Function ====
            Console.WriteLine("\nWelke soort zou je willen verwijderen?");

            for (int i = 0; i < soorten.Count; i++) {
                Console.WriteLine($"{i + 1}. Soort ID: {soorten[i].SoortID}, Naam: {soorten[i].Naam}");
            }

            while (true) {
                invoer = Console.ReadLine();

                if (int.TryParse(invoer, out keuze)) {
                    if (keuze >= 1 && keuze <= soorten.Count) {
                        break;
                    }
                    Console.WriteLine($"Ongeldige keuze. Kies een nummer tussen 1 en {soorten.Count}.");
                }
            }

            await soortInstance.DeleteSoort(soorten[keuze - 1].SoortID);
        }

        // ======== Main ========
        static async Task Main(string[] args) {
            // ==== Start of Main ====
            while (true) {
                Console.Write(""""
                              "Wat wil je doen binnen de applicatie?
                              
                              === Waarnemingen ===
                              1. Bekijk alle waarnemingen
                              2. Waarneming toevoegen
                              3. Waarneming veranderen
                              4. Waarneming verwijderen
                              
                              === Soorten ===
                              5. Bekijk alle soorten
                              6. Soort toevoegen
                              7. Soort veranderen
                              8. Soort verwijderen
                              
                              """");

                string invoer = Console.ReadLine();
                if (int.TryParse(invoer, out int keuze)) {
                    if (keuze is < 1 or > 8) { Console.WriteLine("Ongeldige keuze. Kies een nummer tussen 1 en 8."); continue; }

                    switch (keuze) {
                        case 1:
                            await ToonWaarnemingen();
                            break;
                        case 2:
                            await VoegWaarnemingToe();
                            break;
                        case 3:
                            await UpdateWaarneming();
                            break;
                        case 4:
                            await DeleteWaarneming();
                            break;
                        case 5:
                            await LaatSoortenZien();
                            break;
                        case 6:
                            await VoegSoortToe();
                            break;
                        case 7:
                            await UpdateSoort();
                            break;
                        case 8:
                            await DeleteSoort();
                            break;
                    }
                }
            }
        }
    }
}
