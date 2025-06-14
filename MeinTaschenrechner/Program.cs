using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MeinKomplexerTaschenrechner
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("-----------------------------------");
            Console.WriteLine("  Komplexer C# Taschenrechner      ");
            Console.WriteLine(" (Unterstützt Ausdrücke wie '4+4', '10*5-2', '2*(3+4)')");
            Console.WriteLine("-----------------------------------");
            Console.WriteLine();

            while (true)
            {
                Console.Write("Geben Sie einen Ausdruck ein (z.B. 2+3*4, (10-2)/2) oder 'exit' zum Beenden: ");
                string input = Console.ReadLine();

                if (input.ToLower() == "exit")
                {
                    Console.WriteLine("Taschenrechner wird beendet. Auf Wiedersehen!");
                    break;
                }

                try
                {
                    // Leerzeichen entfernen für einfachere Verarbeitung
                    string cleanInput = input.Replace(" ", "");

                    // Tokenisierung: Zerlegt den String in Zahlen, Operatoren und Klammern
                    Queue<string> tokens = Tokenize(cleanInput);

                    // Shunting-Yard-Algorithmus: Konvertiert den Infix-Ausdruck in RPN
                    Queue<string> rpnQueue = ShuntingYard(tokens);

                    // RPN-Auswertung: Berechnet das Ergebnis des RPN-Ausdrucks
                    double result = EvaluateRPN(rpnQueue);

                    Console.WriteLine($"Ergebnis von '{input}': {result}");
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"Fehler bei der Eingabe: {ex.Message}");
                }
                catch (DivideByZeroException)
                {
                    Console.WriteLine("Fehler: Division durch Null ist nicht erlaubt.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ein unerwarteter Fehler ist aufgetreten: {ex.Message}");
                }
                Console.WriteLine("-----------------------------------");
                Console.WriteLine();
            }
        }

        // Tokenisiert den Eingabe-String in eine Warteschlange von Token (Zahlen, Operatoren, Klammern)
        static Queue<string> Tokenize(string expression)
        {
            // Regex, um Zahlen (ganze Zahlen oder Dezimalzahlen), Operatoren und Klammern zu finden
            // Wichtig: negative Zahlen am Anfang oder nach einer öffnenden Klammer behandeln
            // Beispiel: -5, (-5), 3*-5
            // Die Regex versucht, längste Matches zuerst zu finden.
            // Der erste Teil `\d+(\.\d+)?` matcht Zahlen
            // Der zweite Teil `[+\-*/()]` matcht Operatoren und Klammern
            // Der dritte Teil `(?<!\d)-` matcht ein Minus, das keine Zahl vorausgeht (für negative Zahlen)
            // oder `(?<=\()[+\-*/]` matcht Operatoren nach öffnender Klammer
            // Für diesen einfachen Fall:
            // Wir zerlegen nur in Zahlen, Operatoren und Klammern. Negative Zahlen am Anfang oder nach Klammern müssen
            // ggf. später als unäre Operatoren oder Teil der Zahl behandelt werden.
            // Für den Anfang können wir es so handhaben, dass wir den Ausdruck später normalisieren.
            // Ein einfacher Regex, der alle Operatoren, Klammern und Zahlen isoliert:
            var tokens = new Queue<string>();
            var regex = new Regex(@"(\d+(\.\d+)?|[+\-*/()]|(?<!\d)-)"); // Updated regex for basic numbers, operators, parens, and leading minus
            var matches = regex.Matches(expression);

            foreach (Match match in matches)
            {
                tokens.Enqueue(match.Value);
            }

            // Normalisierung für unäre Minus-Operatoren
            // Ersetze negatives Vorzeichen am Anfang oder nach öffnender Klammer durch eine Zahl
            // Beispiel: -5 -> 0-5
            // Beispiel: 2*(-3) -> 2*(0-3)
            // Dies ist eine Vereinfachung. Eine robustere Lösung würde einen separaten Token-Typ für unäre Operatoren nutzen.
            // Für diesen Ansatz gehen wir davon aus, dass wir negative Zahlen als "0-Zahl" schreiben.
            // Wenn der Ausdruck mit '-' beginnt oder ein '(' direkt von '-' gefolgt wird,
            // füge eine '0' vor dem '-' ein.
            var tempTokens = new List<string>();
            bool prevWasOperatorOrParen = true; // Annahme: Am Anfang oder nach '('
            foreach (var token in tokens)
            {
                if (token == "-" && prevWasOperatorOrParen)
                {
                    tempTokens.Add("0");
                    tempTokens.Add("-");
                }
                else
                {
                    tempTokens.Add(token);
                }

                prevWasOperatorOrParen = "+-*/(".Contains(token);
            }
            return new Queue<string>(tempTokens);
        }

        // Definiert die Priorität der Operatoren (höhere Zahl = höhere Priorität)
        static int GetPrecedence(string op)
        {
            switch (op)
            {
                case "+":
                case "-":
                    return 1;
                case "*":
                case "/":
                    return 2;
                default:
                    return 0; // Für Klammern oder andere nicht-Operatoren
            }
        }

        // Überprüft, ob ein Token ein Operator ist
        static bool IsOperator(string token)
        {
            return token == "+" || token == "-" || token == "*" || token == "/";
        }

        // Shunting-Yard-Algorithmus: Konvertiert Infix in RPN
        static Queue<string> ShuntingYard(Queue<string> tokens)
        {
            Queue<string> outputQueue = new Queue<string>();
            Stack<string> operatorStack = new Stack<string>();

            while (tokens.Count > 0)
            {
                string token = tokens.Dequeue();

                if (double.TryParse(token, out _)) // Wenn es eine Zahl ist
                {
                    outputQueue.Enqueue(token);
                }
                else if (IsOperator(token)) // Wenn es ein Operator ist
                {
                    while (operatorStack.Count > 0 && IsOperator(operatorStack.Peek()) &&
                           GetPrecedence(operatorStack.Peek()) >= GetPrecedence(token))
                    {
                        outputQueue.Enqueue(operatorStack.Pop());
                    }
                    operatorStack.Push(token);
                }
                else if (token == "(") // Wenn es eine öffnende Klammer ist
                {
                    operatorStack.Push(token);
                }
                else if (token == ")") // Wenn es eine schließende Klammer ist
                {
                    while (operatorStack.Count > 0 && operatorStack.Peek() != "(")
                    {
                        outputQueue.Enqueue(operatorStack.Pop());
                    }
                    if (operatorStack.Count == 0)
                    {
                        throw new ArgumentException("Fehlende öffnende Klammer.");
                    }
                    operatorStack.Pop(); // Entferne die öffnende Klammer vom Stack
                }
                else
                {
                    throw new ArgumentException($"Unbekanntes Token: '{token}'");
                }
            }

            // Verschiebe verbleibende Operatoren vom Stack in die Ausgabe-Queue
            while (operatorStack.Count > 0)
            {
                if (operatorStack.Peek() == "(")
                {
                    throw new ArgumentException("Fehlende schließende Klammer.");
                }
                outputQueue.Enqueue(operatorStack.Pop());
            }

            return outputQueue;
        }

        // Auswertung der Reverse Polish Notation (RPN)
        static double EvaluateRPN(Queue<string> rpnTokens)
        {
            Stack<double> valueStack = new Stack<double>();

            foreach (string token in rpnTokens)
            {
                if (double.TryParse(token, out double number))
                {
                    valueStack.Push(number);
                }
                else if (IsOperator(token))
                {
                    if (valueStack.Count < 2)
                    {
                        throw new ArgumentException("Ungültiger RPN-Ausdruck: Nicht genügend Operanden für Operator.");
                    }
                    double operand2 = valueStack.Pop();
                    double operand1 = valueStack.Pop();

                    double result = 0;
                    switch (token)
                    {
                        case "+":
                            result = operand1 + operand2;
                            break;
                        case "-":
                            result = operand1 - operand2;
                            break;
                        case "*":
                            result = operand1 * operand2;
                            break;
                        case "/":
                            if (operand2 == 0)
                            {
                                throw new DivideByZeroException();
                            }
                            result = operand1 / operand2;
                            break;
                    }
                    valueStack.Push(result);
                }
                else
                {
                    throw new ArgumentException($"Unbekanntes Token in RPN: '{token}'");
                }
            }

            if (valueStack.Count != 1)
            {
                throw new ArgumentException("Ungültiger RPN-Ausdruck: Mehrere Ergebnisse oder unzureichende Operatoren.");
            }

            return valueStack.Pop();
        }
    }
}