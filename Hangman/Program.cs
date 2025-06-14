using System;
using System.IO;
using System.Collections.Generic;

class Hangman
{
    public static void Main()
    {
        // Pfad zur Wörterdatei. Stelle sicher, dass die Datei dort existiert.
        string dateipfad = "top10000de.txt";
        
        // Zufallsgenerator für die Wortauswahl
        Random choice = new Random();
        
        bool choiceMaking = true; // Steuert die Schleife zum Finden eines gültigen Wortes
        bool playing = true;      // Steuert die Hauptspielschleife
        
        // Variablen für das ausgewählte Wort und die Spielzustände
        string[] wörter = null; 
        string randomWord = null;
        char[] letters = null; // Das eigentliche Wort als Char-Array
        char[] blank = null;   // Das Array, das die Unterstriche und erratenen Buchstaben zeigt
        
        int falscheVersuche = 0;
        // WICHTIG: Max Fehlversuche ist jetzt 9, da es 9 Schritte gibt (0 für Basis, dann 8 für Galgen/Männchen)
        const int MAX_FALSCHE_VERSUCHE = 9; 

        // Liste zum Speichern der bereits geratenen Buchstaben, um Dopplungen zu vermeiden
        List<char> gerateneBuchstaben = new List<char>(); 

        try
        {
            wörter = File.ReadAllLines(dateipfad);
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine($"Fehler: Die Datei '{dateipfad}' wurde nicht gefunden.");
            Console.WriteLine("Bitte stelle sicher, dass die Wörterdatei am korrekten Ort liegt.");
            Console.WriteLine("Drücke eine beliebige Taste zum Beenden.");
            Console.ReadKey();
            return; 
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ein unerwarteter Fehler ist beim Lesen der Datei aufgetreten: {ex.Message}");
            Console.WriteLine("Drücke eine beliebige Taste zum Beenden.");
            Console.ReadKey();
            return;
        }

        // --- Wortauswahl-Logik ---
        while (choiceMaking)
        {
            int pick = choice.Next(0, wörter.Length); 
            randomWord = wörter[pick]; 
            randomWord = randomWord.ToLower(); 
            letters = randomWord.ToCharArray();

            if (letters.Length >= 4 && letters.Length <= 8) 
            {
                 bool isValidWord = true;
                 foreach (char c in letters)
                 {
                     if (!char.IsLetter(c)) 
                     {
                         isValidWord = false;
                         break;
                     }
                 }

                 if (isValidWord)
                 {
                     choiceMaking = false; 
                 }
            }
        }

        // --- Initialisierung des leeren Spielfelds ---
        blank = new char[letters.Length];
        for (int i = 0; i < blank.Length; i++)
        {
            blank[i] = '_'; 
        }

        // --- Hauptspielschleife ---
        while (playing)
        {
            Console.Clear(); 

            // Galgen zeichnen
            ZeichneGalgen(falscheVersuche); 
            
            Console.WriteLine("-----------------------------------");
            Console.WriteLine("              HANGMAN              ");
            Console.WriteLine("-----------------------------------");
            Console.WriteLine($"Das Wort hat {randomWord.Length} Buchstaben.");
            Console.WriteLine("");

            Console.WriteLine(string.Join(" ", blank)); 
            Console.WriteLine("");

            if (gerateneBuchstaben.Count > 0)
            {
                Console.WriteLine($"Bereits geraten: {string.Join(", ", gerateneBuchstaben)}");
            }
            Console.WriteLine($"Verbleibende Versuche: {MAX_FALSCHE_VERSUCHE - falscheVersuche}");
            Console.WriteLine("");

            Console.Write("Gib einen Buchstaben ein: ");
            string rawInput = Console.ReadLine(); 
            char charInput;

            // --- Eingabevalidierung ---
            if (rawInput == null || rawInput.Length != 1 || !char.IsLetter(rawInput[0]))
            {
                Console.WriteLine("Ungültige Eingabe. Bitte gib genau EINEN Buchstaben ein.");
                System.Threading.Thread.Sleep(1500); 
                continue; 
            }

            charInput = char.ToLower(rawInput[0]); 

            // Prüfen, ob der Buchstabe bereits geraten wurde
            if (gerateneBuchstaben.Contains(charInput))
            {
                Console.WriteLine($"Du hast '{charInput}' bereits geraten. Versuche einen anderen Buchstaben.");
                System.Threading.Thread.Sleep(1500);
                continue; 
            }

            gerateneBuchstaben.Add(charInput);

            // --- Logik zum Prüfen des Buchstabens und Aktualisieren des Spielfelds ---
            bool foundLetter = false;
            for (int i = 0; i < letters.Length; i++)
            {
                if (letters[i] == charInput)
                {
                    blank[i] = charInput; 
                    foundLetter = true;
                }
            }

            if (foundLetter)
            {
                Console.WriteLine($"Sehr gut! '{charInput}' ist im Wort enthalten.");
            }
            else
            {
                Console.WriteLine($"Leider! '{charInput}' ist NICHT im Wort enthalten.");
                falscheVersuche++; 
            }

            // --- Sieg- und Verlustbedingungen prüfen ---
            if (!new string(blank).Contains('_'))
            {
                Console.Clear();
                ZeichneGalgen(falscheVersuche); 
                Console.WriteLine("-----------------------------------");
                Console.WriteLine("              GEWONNEN!            ");
                Console.WriteLine("-----------------------------------");
                Console.WriteLine($"Glückwunsch! Du hast das Wort erraten: {randomWord.ToUpper()}");
                playing = false; 
            }
            else if (falscheVersuche >= MAX_FALSCHE_VERSUCHE) 
            {
                Console.Clear();
                ZeichneGalgen(falscheVersuche); 
                Console.WriteLine("-----------------------------------");
                Console.WriteLine("              VERLOREN!            ");
                Console.WriteLine("-----------------------------------");
                Console.WriteLine($"Leider hast du verloren. Das Wort war: {randomWord.ToUpper()}");
                playing = false; 
            }
            
            System.Threading.Thread.Sleep(1000); 
        }

        Console.WriteLine("\nDrücke eine beliebige Taste, um das Spiel zu beenden...");
        Console.ReadKey(); 
    }

    // --- Methode zum Zeichnen des Galgens basierend auf den Fehlversuchen ---
    static void ZeichneGalgen(int falscheVersuche)
    {
        // Konstanten für die Galgen-Teile zur besseren Lesbarkeit
        const string LEERZEILE = "     ";
        const string OBERER_BALKEN = "-----";
        const string VERTIKALER_PFOSTEN = "|";
        const string GALGEN_SEIL = "|   |";
        const string PERSON_KOPF = "|   O";
        const string PERSON_RUMPF = "|   |";
        const string PERSON_ARML = "|  /|";
        const string PERSON_ARMR = "|  /|\\";
        const string PERSON_BEINL = "|  /";
        const string PERSON_BEINR = "|  / \\";
        const string BASIS = "-------";

        // Array für die 4 Zeilen über der Basis, die sich ändern
        string[] galgenLinien = new string[4];

        // Standardmäßig sind alle Linien leer (oder mit dem vertikalen Pfosten, wenn dieser immer da ist)
        // In diesem Szenario starten wir sie wirklich leer, da der Pfosten selbst ein Fehler ist.
        for (int i = 0; i < 4; i++)
        {
            galgenLinien[i] = LEERZEILE; 
        }

        switch (falscheVersuche)
        {
            case 0: // Nur die Basis (keine Fehlversuche)
                // Die galgenLinien bleiben hier leer, es wird nur die Basis ausgegeben.
                break;
            case 1: // Vertikalbalken (erster Fehler)
                for (int i = 0; i < 4; i++)
                {
                    galgenLinien[i] = VERTIKALER_PFOSTEN + LEERZEILE;
                }
                break;
            case 2: // Galgenstamm (Vertikalbalken + Oberer Querbalken)
                galgenLinien[0] = VERTIKALER_PFOSTEN + OBERER_BALKEN;
                galgenLinien[1] = VERTIKALER_PFOSTEN + LEERZEILE;
                galgenLinien[2] = VERTIKALER_PFOSTEN + LEERZEILE;
                galgenLinien[3] = VERTIKALER_PFOSTEN + LEERZEILE;
                break;
            case 3: // Seil
                galgenLinien[0] = VERTIKALER_PFOSTEN + OBERER_BALKEN;
                galgenLinien[1] = GALGEN_SEIL;
                galgenLinien[2] = VERTIKALER_PFOSTEN + LEERZEILE;
                galgenLinien[3] = VERTIKALER_PFOSTEN + LEERZEILE;
                break;
            case 4: // Kopf
                galgenLinien[0] = VERTIKALER_PFOSTEN + OBERER_BALKEN;
                galgenLinien[1] = PERSON_KOPF;
                galgenLinien[2] = VERTIKALER_PFOSTEN + LEERZEILE;
                galgenLinien[3] = VERTIKALER_PFOSTEN + LEERZEILE;
                break;
            case 5: // Rumpf
                galgenLinien[0] = VERTIKALER_PFOSTEN + OBERER_BALKEN;
                galgenLinien[1] = PERSON_KOPF;
                galgenLinien[2] = PERSON_RUMPF;
                galgenLinien[3] = VERTIKALER_PFOSTEN + LEERZEILE;
                break;
            case 6: // Linker Arm
                galgenLinien[0] = VERTIKALER_PFOSTEN + OBERER_BALKEN;
                galgenLinien[1] = PERSON_KOPF;
                galgenLinien[2] = PERSON_ARML;
                galgenLinien[3] = VERTIKALER_PFOSTEN + LEERZEILE;
                break;
            case 7: // Rechter Arm
                galgenLinien[0] = VERTIKALER_PFOSTEN + OBERER_BALKEN;
                galgenLinien[1] = PERSON_KOPF;
                galgenLinien[2] = PERSON_ARMR;
                galgenLinien[3] = VERTIKALER_PFOSTEN + LEERZEILE;
                break;
            case 8: // Linkes Bein
                galgenLinien[0] = VERTIKALER_PFOSTEN + OBERER_BALKEN;
                galgenLinien[1] = PERSON_KOPF;
                galgenLinien[2] = PERSON_ARMR;
                galgenLinien[3] = PERSON_BEINL;
                break;
            case 9: // Rechtes Bein (Verlust-Zustand, letztes Glied)
                galgenLinien[0] = VERTIKALER_PFOSTEN + OBERER_BALKEN;
                galgenLinien[1] = PERSON_KOPF;
                galgenLinien[2] = PERSON_ARMR;
                galgenLinien[3] = PERSON_BEINR;
                break;
            default: // Dies sollte nur bei mehr Fehlern als MAX_FALSCHE_VERSUCHE auftreten
                galgenLinien[0] = VERTIKALER_PFOSTEN + OBERER_BALKEN;
                galgenLinien[1] = PERSON_KOPF;
                galgenLinien[2] = PERSON_ARMR;
                galgenLinien[3] = PERSON_BEINR;
                break;
        }

        // Gib die konstruierten Zeilen aus
        foreach (string line in galgenLinien)
        {
            Console.WriteLine(line);
        }
        Console.WriteLine(BASIS); // Die Basis kommt immer darunter
        Console.WriteLine(""); // Leerzeile für bessere Lesbarkeit
    }
}