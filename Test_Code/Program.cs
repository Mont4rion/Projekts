using System;

class Program // Fehler: Klammern nach dem Klassennamen entfernt
{
    static int a = 0;
    static bool Test = false;

    // Main-Methode muss statisch sein, um direkt aufgerufen zu werden
    public static void Main()
    {
        Bool_Check();

        while (a < 100)
        {
            a++;
            Bool_Check();
            if (Test == true)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(a);
            }

            else
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine(a);
            }
        }
    }

    public static void Bool_Check()
    {
        if (a % 2 == 0)
        {
            Test = true;
        }

        else
        {
            Test = false;
        }
    }
}