using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;

class Autoclicker
{
    [DllImport("user32.dll")]
    static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, IntPtr dwExtraInfo);

    [DllImport("user32.dll")]
    static extern short GetAsyncKeyState(int vKey);  // hotkey

    [DllImport("gdi32.dll")]
    static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

    private const int LOGPIXELSX = 88;
    private const int LOGPIXELSY = 90;

    const uint LEFTDOWN = 0x02;
    const uint LEFTUP = 0x04; // Notwendig für einen vollständigen Klick
    private static int hotkey = 0x26; // Standardmäßig: Oben Pfeiltaste
    private static string hotkeyName = "Oben Pfeiltaste"; // Klarere Bezeichnung
    private const int CHANGE_HOTKEY = 0x70; // F1 Taste für Hotkey-Änderung
    private const string CHANGE_HOTKEY_NAME = "F1";
    private const int CHANGE_SPEED = 0x71; // F2 Taste für Geschwindigkeitsänderung
    private const string CHANGE_SPEED_NAME = "F2";
    private const int MIN_INTERVAL = 1;
    private const int MAX_INTERVAL = 1000; // Beispiel: 1 Sekunde maximal

    private static volatile bool enableClicker = false; // volatile für Thread-Sicherheit
    private static volatile int clickerInterval = 5; // volatile für Thread-Sicherheit
    private static ManualResetEvent clickEvent = new ManualResetEvent(false); // Für präzises Timing

    static void Main(string[] args)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        DisplayInfo();

        // Starte den Hotkey-Überwachungs-Thread
        Thread hotkeyThread = new Thread(HotkeyMonitor);
        hotkeyThread.IsBackground = true; // Beendet sich mit der Hauptanwendung
        hotkeyThread.Start();

        // Hauptschleife für das Klicken
        while (true)
        {
            if (enableClicker)
            {
                MouseClick();
            }
            clickEvent.WaitOne(clickerInterval); // Warte für das Intervall oder bis ein Signal kommt
        }
    }

    static void HotkeyMonitor()
    {
        while (true)
        {
            // Hotkey zum Ändern des Hotkeys
            if (GetAsyncKeyState(CHANGE_HOTKEY) < 0)
            {
                Console.WriteLine("Drücke jetzt die neue Taste für den Hotkey...");
                Thread.Sleep(1000);
                int newHotkey = WaitForNewHotkey();
                if (newHotkey != 0 && newHotkey != CHANGE_HOTKEY && newHotkey != CHANGE_SPEED)
                {
                    hotkey = newHotkey;
                    hotkeyName = GetKeyName(newHotkey);
                    // Interaktion mit der Hauptkonsole muss synchronisiert sein, aber hier ist es einfach
                    Console.Clear();
                    DisplayInfo();
                    Console.WriteLine($"Hotkey erfolgreich zu '{hotkeyName}' geändert.");
                }
                else if (newHotkey == CHANGE_HOTKEY || newHotkey == CHANGE_SPEED)
                {
                    Console.WriteLine($"'{GetKeyName(newHotkey)}' kann nicht als Hotkey verwendet werden.");
                }
                else
                {
                    Console.WriteLine("Keine gültige Taste gedrückt.");
                }
                Thread.Sleep(500);
            }
            // Hotkey zum Ändern der Klickgeschwindigkeit
            else if (GetAsyncKeyState(CHANGE_SPEED) < 0)
            {
                ChangeClickInterval();
                // Interaktion mit der Hauptkonsole muss synchronisiert sein, aber hier ist einfach
                Console.Clear();
                DisplayInfo();
            }
            // Normaler Hotkey zum Aktivieren/Deaktivieren
            else if (GetAsyncKeyState(hotkey) < 0)
            {
                enableClicker = !enableClicker;
                Console.WriteLine($"Klicker ist jetzt {(enableClicker ? "AKTIVIERT" : "DEAKTIVIERT")}.");
                Thread.Sleep(300);
            }

            Thread.Sleep(10); // Kurze Pause im Hotkey-Thread, um CPU-Last zu reduzieren
        }
    }

    static void DisplayInfo()
    {
        Console.WriteLine("----------------------------------");
        Console.WriteLine("    Automatische Mausklicker");
        Console.WriteLine("----------------------------------");
        Console.WriteLine($"Hotkey zum Aktivieren/Deaktivieren: {hotkeyName}");
        Console.WriteLine($"Aktuelles Klick-Intervall: {clickerInterval} Millisekunden (Bereich: {MIN_INTERVAL}-{MAX_INTERVAL} ms)");
        Console.WriteLine($"Drücke {CHANGE_HOTKEY_NAME} um den Hotkey zu ändern.");
        Console.WriteLine($"Drücke {CHANGE_SPEED_NAME} um die Klickgeschwindigkeit zu ändern.");

        // DPI-Informationen abrufen und anzeigen
        using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
        {
            IntPtr hdc = g.GetHdc();
            int dpiX = GetDeviceCaps(hdc, LOGPIXELSX);
            int dpiY = GetDeviceCaps(hdc, LOGPIXELSY);
            g.ReleaseHdc(hdc);
            Console.WriteLine($"Bildschirm DPI: X={dpiX}, Y={dpiY}");
        }
        Console.WriteLine("----------------------------------");
        Console.WriteLine("Klicker ist initial deaktiviert.");
        Console.WriteLine("Drücke den Hotkey zum Starten/Stoppen.");
        Console.WriteLine("----------------------------------");
    }

    static void ChangeClickInterval()
    {
        Console.WriteLine("----------------------------------");
        Console.WriteLine($"Aktuelles Klick-Intervall: {clickerInterval} Millisekunden");
        Console.WriteLine($"Bitte gib das neue Klick-Intervall in Millisekunden ein ({MIN_INTERVAL}-{MAX_INTERVAL}) und drücke Enter:");

        // Workaround, um den Eingabepuffer zu leeren (nicht perfekt)
        while (Console.KeyAvailable)
        {
            Console.ReadKey(true);
        }

        string? input = Console.ReadLine();

        if (input != null)
        {
            if (int.TryParse(input, out int newInterval))
            {
                if (newInterval >= MIN_INTERVAL && newInterval <= MAX_INTERVAL)
                {
                    clickerInterval = newInterval;
                    Console.WriteLine($"Klick-Intervall erfolgreich auf {clickerInterval} ms geändert.");
                }
                else
                {
                    Console.WriteLine($"Ungültiger Wert. Das Intervall muss zwischen {MIN_INTERVAL} und {MAX_INTERVAL} liegen.");
                }
            }
            else
            {
                Console.WriteLine("Ungültige Eingabe. Bitte gib eine Zahl ein.");
            }
        }
        else
        {
            Console.WriteLine("Fehler beim Lesen der Eingabe.");
        }
        Console.WriteLine("----------------------------------");
        Thread.Sleep(500);
    }

    static void MouseClick()
    {
        mouse_event(LEFTDOWN, 0, 0, 0, IntPtr.Zero);
        mouse_event(LEFTUP, 0, 0, 0, IntPtr.Zero);
    }

    static int WaitForNewHotkey()
    {
        while (true)
        {
            for (int i = 1; i < 256; i++)
            {
                if (GetAsyncKeyState(i) < 0)
                {
                    return i;
                }
            }
            Thread.Sleep(10);
        }
    }

    static string GetKeyName(int keyCode)
    {
        if (keyCode >= 0x30 && keyCode <= 0x39) return ((char)keyCode).ToString(); // Zahlen 0-9
        if (keyCode >= 0x41 && keyCode <= 0x5A) return ((char)keyCode).ToString(); // Buchstaben A-Z

        // Sonderfälle
        switch (keyCode)
        {
            case 0x01: return "Linke Maustaste";
            case 0x02: return "Rechte Maustaste";
            case 0x04: return "Mittlere Maustaste";
            case 0x08: return "Backspace";
            case 0x09: return "Tab";
            case 0x0D: return "Enter";
            case 0x10: return "Umschalt (Shift)";
            case 0x11: return "Strg (Ctrl)";
            case 0x12: return "Alt";
            case 0x14: return "Feststelltaste (Caps Lock)";
            case 0x1B: return "Esc";
            case 0x20: return "Leertaste";
            case 0x21: return "Bild Auf (Page Up)";
            case 0x22: return "Bild Ab (Page Down)";
            case 0x23: return "Ende";
            case 0x24: return "Pos1 (Home)";
            case 0x25: return "Pfeil links";
            case 0x26: return "Pfeil oben";
            case 0x27: return "Pfeil rechts";
            case 0x28: return "Pfeil unten";
            case 0x2D: return "Einfügen (Insert)";
            case 0x2E: return "Entfernen (Delete)";
            case 0x70: return "F1";
            case 0x71: return "F2";
            case 0x72: return "F3";
            case 0x73: return "F4";
            case 0x74: return "F5";
            case 0x75: return "F6";
            case 0x76: return "F7";
            case 0x77: return "F8";
            case 0x78: return "F9";
            case 0x79: return "F10";
            case 0x7A: return "F11";
            case 0x7B: return "F12";
            default:
                return $"Taste mit Code 0x{keyCode:X}";
        }
    }
}