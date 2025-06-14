using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks; // Wichtig für async/await
using Newtonsoft.Json;
using Newtonsoft.Json.Linq; // Für die Arbeit mit JSON-Objekten

class Zaheln_raten
{
    // static Random NumberGenerator = new Random(); // Nicht mehr benötigt, da wir die API nutzen
    static int Number; // Die gesuchte Zahl von der API
    static int Guess;
    static int Score;
    static int LastScore;
    static int ScoreDif = 0;
    static bool Playing = false; 
    static bool keepPlayingOverall = true; 
    static bool FirstRound = true;
    static string Start;
    static string Input;
    static string End;
    static string ScoreOutput = "";
    static bool exitedEarly = false;

    // --- API-SPEZIFISCHE VARIABLEN ---
    private static readonly HttpClient _httpClient = new HttpClient();
    private const string ApiBaseUrl = "https://api.random.org/json-rpc/4/invoke"; // Endpunkt für JSON-RPC
    private const string YourApiKey = "3bd94629-82e5-4b5f-b155-1ea17707984d"; // IHR API-SCHLÜSSEL

    // --- MAIN METHODE WIRD NUN ASYNCHRON ---
    static async Task Main() // Hinzugefügt: async Task
    {
        Console.WriteLine("Gebe Start ein um das Spiel zu beginnen");
        Start = Console.ReadLine();

        if (Start.ToLower() == "start") // .ToLower() für case-insensitive "start"
        {
            Playing = true;
            keepPlayingOverall = true; 
            Score = 0;

            // --- ERSTER API-AUFRUF HIER ---
            // Versuche, die erste Zahl von der API zu holen
            Number = await GetRandomIntegerFromApi(1, 100); 
            if (Number == -1) // Fehlercode für API-Problem
            {
                Console.WriteLine("Konnte die erste Zufallszahl nicht von random.org abrufen. Spiel beendet.");
                Console.ReadKey();
                return;
            }
            // --- ENDE API-AUFRUF ---

            Console.WriteLine("Ich habe mir eine Zahl zwischen 1 und 99 ausgedacht. Rate mal!");
        }
        else if (Start.ToLower() == "exit") // .ToLower() für case-insensitive "exit"
        {
            Console.WriteLine("Danke fürs Spielen! Auf Wiedersehen.");
            Console.ReadKey();
            return; 
        }
        else
        {
            Console.WriteLine("Ungültige Eingabe. Gebe 'Start' ein, um das Spiel zu beginnen, oder 'Exit' zum Beenden.");
            Console.ReadKey();
            return; 
        }

        while (keepPlayingOverall) // Die äußere Schleife für "Nochmal spielen"
        {
            Playing = true;
            
            // Score der vorherigen Runde speichern, BEVOR der aktuelle Score zurückgesetzt wird
            if (!FirstRound) 
            {
                LastScore = Score; 
            }
            
            Score = 0; // Score für die neue Runde zurücksetzen
            Console.WriteLine("\n--- Neue Runde ---"); 
            
            // --- API-AUFRUF FÜR JEDE NEUE RUNDE ---
            Number = await GetRandomIntegerFromApi(1, 100); // Holen Sie eine neue Zahl für jede Runde
            if (Number == -1) // Fehlerbehandlung, falls API-Aufruf fehlschlägt
            {
                Console.WriteLine("Konnte die Zufallszahl für die neue Runde nicht von random.org abrufen. Spiel beendet.");
                keepPlayingOverall = false; // Spiel beenden
                break; // Schleife verlassen
            }
            // --- ENDE API-AUFRUF ---

            Console.WriteLine("Ich habe mir eine Zahl zwischen 1 und 99 ausgedacht. Rate mal!");

            while (Playing == true)
            {
                Console.WriteLine($"Du hast {Score} Versuche genutzt");
                Console.WriteLine("Gebe eine Nummer ein, um die Gesuchte Zahl zu finden (zwischen 1 und 99):"); 

                Input = Console.ReadLine();

                if (int.TryParse(Input, out Guess))
                {
                    if (Guess < Number)
                    {
                        Console.WriteLine("Die Nummer ist größer als dein letzter Versuch.");
                        Score++;
                    }
                    else if (Guess > Number)
                    {
                        Console.WriteLine("Die Nummer ist kleiner als dein letzter Versuch.");
                        Score++;
                    }
                    else // Guess == Number
                    {
                        Console.WriteLine($"Herzlichen Glückwunsch! Du hast die Zahl {Number} erraten!");
                        Playing = false; // Spieler hat gewonnen, beende die aktuelle Runde
                    }
                }
                else if (Input.ToLower() == "exit")
                {
                    exitedEarly = true; 
                    Playing = false; 
                    keepPlayingOverall = false; 
                }
                else
                {
                    Console.WriteLine("Ungültige Eingabe. Bitte gib eine Zahl ein oder 'Exit'.");
                }
            }

            if (keepPlayingOverall)
            {
                if (!FirstRound)
                {
                    LastRechner();
                    Console.WriteLine($"Du hast {Score} Versuche gebraucht. {ScoreOutput}");
                }
                else
                {
                    Console.WriteLine($"Du hast {Score} Versuche gebraucht.");
                }

                FirstRound = false;

                Console.WriteLine("\nDrucke Enter, um nochmal zu spielen, oder schreibe 'Exit'.");
                End = Console.ReadLine();

                if (End.ToLower() == "exit")
                {
                    keepPlayingOverall = false; 
                }
            }
        } 

        if (!exitedEarly)
        {
            Console.WriteLine("Danke fürs Spielen! Auf Wiedersehen.");
        }
        Console.ReadKey(); 
    }

    static void LastRechner()
    {
        ScoreDif = Math.Abs(LastScore - Score); 

        if (Score < LastScore) 
        {
            ScoreOutput = ($"Du warst {ScoreDif} Versuche schneller als in der letzten Runde!");
        }
        else if (Score > LastScore) 
        {
            ScoreOutput = ($"Du warst leider {ScoreDif} Versuche langsamer als in der letzten Runde.");
        }
        else
        {
            ScoreOutput = "Du warst genauso schnell wie in der letzten Runde!";
        }
    }

    // --- NEUE METHODE ZUR ABFRAGE DER RANDOM.ORG API ---
    static async Task<int> GetRandomIntegerFromApi(int min, int max)
    {
        if (string.IsNullOrWhiteSpace(YourApiKey))
        {
            Console.WriteLine("Fehler: Der API-Schlüssel ist leer oder ungültig. Bitte stellen Sie sicher, dass Ihr API-Schlüssel korrekt eingefügt ist.");
            return -1; // Fehlercode
        }

        try
        {
            var requestBody = new
            {
                jsonrpc = "2.0",
                method = "generateIntegers",
                @params = new
                {
                    apiKey = YourApiKey,
                    n = 1,   // Wir brauchen nur EINE Zahl
                    min = min,
                    max = max,
                    replacement = true, 
                },
                id = 1 
            };

            string jsonRequest = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            Console.WriteLine("Fordere eine Zufallszahl von random.org an...");
            // Console.WriteLine($"Anfrage-Payload: {jsonRequest}"); // Kann für Debugging aktiviert werden

            HttpResponseMessage response = await _httpClient.PostAsync(ApiBaseUrl, content);
            response.EnsureSuccessStatusCode(); 

            string responseBody = await response.Content.ReadAsStringAsync();
            // Console.WriteLine($"Antwort von der API: {responseBody}"); // Kann für Debugging aktiviert werden

            JObject jsonResponse = JObject.Parse(responseBody);

            if (jsonResponse["error"] != null)
            {
                Console.WriteLine($"\nAPI-Fehler erhalten: {jsonResponse["error"]["message"]}");
                return -1; // Fehlercode
            }

            JArray data = (JArray)jsonResponse["result"]["random"]["data"];
            if (data.Count > 0)
            {
                return (int)data[0]; // Die erste und einzige Zahl zurückgeben
            }
            else
            {
                Console.WriteLine("Fehler: API hat keine Zahl zurückgegeben.");
                return -1; // Fehlercode
            }
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"\nHTTP-Anfragefehler beim Abrufen der Zahl: {e.Message}");
            if (e.StatusCode.HasValue)
            {
                Console.WriteLine($"HTTP Statuscode: {e.StatusCode}");
            }
            return -1; // Fehlercode
        }
        catch (JsonException e)
        {
            Console.WriteLine($"\nFehler beim Parsen der JSON-Antwort: {e.Message}");
            return -1; // Fehlercode
        }
        catch (Exception e)
        {
            Console.WriteLine($"\nEin unerwarteter Fehler ist aufgetreten: {e.Message}");
            return -1; // Fehlercode
        }
    }
}