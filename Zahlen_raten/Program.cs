using System;

class Zaheln_raten
{
    static Random NumberGenerator = new Random();
    static int Number;
    static int Guess;
    static int Score;
    static int LastScore;
    static int ScoreDif = 0; // Initialisiert
    static bool Playing = false; // Steuert die innere Rate-Schleife
    static bool keepPlayingOverall = true; // <--- HIER GEÄNDERT: Initial auf true
    static bool FirstRound = true;
    static string Start;
    static string Input;
    static string End;
    static string ScoreOutput = ""; // Initialisiert
    static bool exitedEarly = false; // Neue Variable, um den frühen Exit-Status zu verfolgen

    static void Main()
    {

        Console.WriteLine("Gebe Start ein um das Spiel zu beginnen");
        Start = Console.ReadLine();

        if (Start == "Start")
        {
            // Setzt Playing und keepPlayingOverall für den ersten Start
            Playing = true;
            keepPlayingOverall = true; 
            Number = NumberGenerator.Next(1, 100); // Zahlen von 1 bis 99
            Score = 0;
            Console.WriteLine("Ich habe mir eine Zahl zwischen 1 und 99 ausgedacht. Rate mal!");
        }
        else if (Start == "Exit")
        {
            Console.WriteLine("Danke fürs Spielen! Auf Wiedersehen.");
            Console.ReadKey();
            return; // Beendet das Programm sofort.
        }
        else
        {
            Console.WriteLine("Ungültige Eingabe. Gebe 'Start' ein, um das Spiel zu beginnen, oder 'Exit' zum Beenden.");
            Console.ReadKey(); // Hält Konsole offen, damit Benutzer Fehlermeldung sieht
            return; // Beendet das Programm, wenn die erste Eingabe ungültig ist
        }


        while (keepPlayingOverall) // Die äußere Schleife für "Nochmal spielen"
        {
            // Diese Variablen müssen FÜR JEDE NEUE RUNDE innerhalb der Schleife zurückgesetzt werden.
            Playing = true;
            Number = NumberGenerator.Next(1, 100); // Zahlen von 1 bis 99
            
            // Score der vorherigen Runde speichern, BEVOR der aktuelle Score zurückgesetzt wird
            // Dies passiert erst ab der ZWEITEN Runde
            if (!FirstRound) // Also, wenn es NICHT die erste Runde ist
            {
                LastScore = Score; 
            }
            
            Score = 0; // Score für die neue Runde zurücksetzen
            Console.WriteLine("\n--- Neue Runde ---"); // Visuelle Trennung für neue Runde
            Console.WriteLine("Ich habe mir eine Zahl zwischen 1 und 99 ausgedacht. Rate mal!");

            // Dies ist die innere Schleife für eine einzelne Spielrunde.
            while (Playing == true)
            {
                Console.WriteLine($"Du hast {Score} versuche genutzt");
                Console.WriteLine("Gebe eine Nummer ein, um die Gesuchte Zahl zu finden (zwischen 1 und 99):"); // Text angepasst

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
                else if (Input.ToLower() == "exit") // .ToLower() für case-insensitive "exit"
                {
                    exitedEarly = true; // Setze den Status für frühen Exit
                    Playing = false;            // Beende die aktuelle Runde
                    keepPlayingOverall = false; // Beende die gesamte Spielschleife
                }
                else
                {
                    Console.WriteLine("Ungültige Eingabe. Bitte gib eine Zahl ein oder 'Exit'.");
                }
            }

            // Dieser Block wird ausgeführt, nachdem eine Runde beendet ist (gewonnen oder 'Exit' gedrückt)
            if (keepPlayingOverall) // Prüfe, ob wir das Spiel nicht schon komplett beenden wollen
            {
                // LastRechner nur aufrufen, wenn es nicht die erste Runde ist und das Spiel noch läuft
                if (!FirstRound)
                {
                    LastRechner();
                    Console.WriteLine($"Du hast {Score} Versuche gebraucht. {ScoreOutput}");
                }
                else
                {
                    // Erste Runde, nur den aktuellen Score anzeigen
                    Console.WriteLine($"Du hast {Score} Versuche gebraucht.");
                }

                FirstRound = false; // Nach der ersten Runde ist dies nicht mehr die erste Runde

                Console.WriteLine("\nDrucke Enter, um nochmal zu spielen, oder schreibe 'Exit'.");
                End = Console.ReadLine();

                if (End.ToLower() == "exit") // .ToLower() für case-insensitive "exit"
                {
                    keepPlayingOverall = false; // Setze auf false, um die äußere Schleife zu beenden
                }
                // Wenn Enter gedrückt wird oder etwas anderes als "exit", läuft die äußere Schleife weiter.
            }
        } // Ende der äußeren 'while (keepPlayingOverall)' Schleife

        // Dieser Block wird nur ausgeführt, wenn 'keepPlayingOverall' auf 'false' gesetzt wurde
        // und das Programm nicht bereits durch einen frühen "Exit" beendet wurde.
        if (!exitedEarly) // Zeige die Abschiedsnachricht nur, wenn wir nicht schon durch einen frühen "exit" abgesprungen sind
        {
            Console.WriteLine("Danke fürs Spielen! Auf Wiedersehen.");
        }
        Console.ReadKey(); // Hält das Konsolenfenster offen, bis eine Taste gedrückt wird.
    }

    static void LastRechner()
    {
        // ScoreDif sollte den ABSOLUTEN Unterschied zeigen
        ScoreDif = Math.Abs(LastScore - Score); 

        // Logik umgedreht: kleinerer aktueller Score bedeutet SCHNELLER
        if (Score < LastScore) 
        {
            ScoreOutput = ($"Du warst {ScoreDif} Versuche schneller als in der letzten Runde!");
        }
        // größerer aktueller Score bedeutet LANGSAMER
        else if (Score > LastScore) 
        {
            ScoreOutput = ($"Du warst leider {ScoreDif} Versuche langsamer als in der letzten Runde.");
        }
        else
        {
            ScoreOutput = "Du warst genauso schnell wie in der letzten Runde!";
        }
    }
}