using System;
using System.Globalization;
using System.IO;

namespace PizzaHyattsville
{
    internal static class Program
    {
        // City is a 4x4 grid labelled with letters A..P (row-major)
        // Row 1 is the BOTTOM of the map; Column 1 is the LEFT edge.
        private const int Rows = 4;
        private const int Cols = 4;
        private static readonly char[,] City = new char[Rows, Cols];
        private static readonly Random Rng = new Random();
        private static string LogPath = "";

        static void Main()
        {
            Console.Title = "PIZZA — Deliver Pizzas in Hyattsville";

            // Build map A..P in row-major order, with row 1 at bottom
            int k = 0;
            for (int r = 0; r < Rows; r++)
                for (int c = 0; c < Cols; c++)
                    City[r, c] = (char)('A' + (k++));

            // Logging
            LogPath = Path.Combine(AppContext.BaseDirectory,
                $"pizza-log-{DateTime.Now:yyyyMMdd-HHmmss}.txt");
            using (var w = File.CreateText(LogPath))
            {
                w.WriteLine($"=== PIZZA LOG === {DateTime.Now}");
            }

            Console.WriteLine("PIZZA DELIVERY GAME\n");
            var name = ReadLineNonEmpty("WHAT IS YOUR FIRST NAME? ");
            Console.WriteLine($"\nHI, {name.ToUpper()}. IN THIS GAME YOU ARE TO TAKE ORDERS");
            Console.WriteLine("FOR PIZZAS. THEN YOU ARE TO TELL A DELIVERY BOY");
            Console.WriteLine("WHERE TO DELIVER THE ORDERED PIZZAS.\n");

            PrintMap();
            Console.WriteLine("THE ABOVE IS A MAP OF THE HOMES WHERE");
            Console.WriteLine("YOU ARE TO SEND PIZZAS.");
            Console.WriteLine("YOUR JOB IS TO GIVE A TRUCK DRIVER");
            Console.WriteLine("THE LOCATION (ROW,COL) OF THE");
            Console.WriteLine("HOME ORDERING THE PIZZA.");
            Console.WriteLine("TYPE MAP TO SEE THE MAP AGAIN, HELP FOR DIRECTIONS, or QUIT TO EXIT.\n");

            Append($"Clerk={name}");

            // Game loop
            while (true)
            {
                // Pick a random house letter to order
                char customer = (char)('A' + Rng.Next(0, Rows * Cols));
                var (row, col) = Find(customer); // correct address

                Console.WriteLine($"HELLO {name.ToUpper()}'S PIZZA. THIS IS {customer}. PLEASE SEND A PIZZA.");
                while (true)
                {
                    string ans = ReadLineNonEmpty($"DELIVER TO {name}. WHERE DOES {customer} LIVE? (ROW,COL) > ").Trim();

                    if (ans.Equals("QUIT", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("\nO.K. {0}, SEE YOU LATER!", name);
                        Append("QUIT");
                        Console.WriteLine($"\nLog written to: {LogPath}");
                        return;
                    }
                    if (ans.Equals("HELP", StringComparison.OrdinalIgnoreCase))
                    {
                        PrintHelp();
                        continue;
                    }
                    if (ans.Equals("MAP", StringComparison.OrdinalIgnoreCase))
                    {
                        PrintMap();
                        continue;
                    }

                    if (!TryParseRowCol(ans, out int r, out int c))
                    {
                        Console.WriteLine("PLEASE ENTER AS ROW,COL (e.g., 2,3) — NUMBERS 1..4.");
                        continue;
                    }

                    Append($"Order {customer}: attempt -> ({r},{c})");

                    // Who lives at the guessed coordinates?
                    char there = LetterAt(r, c);

                    if (there == customer)
                    {
                        Console.WriteLine($"HELLO {name}. THIS IS {customer}. THANKS FOR THE PIZZA.\n");
                        Append($"Delivered OK to {customer} at ({row},{col})");
                        break; // new order
                    }
                    else
                    {
                        Console.WriteLine($"THIS IS {there}. I DID NOT ORDER A PIZZA.");
                        Console.WriteLine($"I LIVE AT {r},{c}.");
                        Console.WriteLine($"DELIVER TO {name}. WHERE DOES {customer} LIVE? (HINT: TRY AGAIN!)\n");
                        // keep asking for the same order until correct
                    }
                }
            }
        }

        // ------- Map & helpers -------

        // Find (row,col) for a letter (1-based rows/cols, row 1 is bottom)
        private static (int row, int col) Find(char letter)
        {
            for (int r = 0; r < Rows; r++)
                for (int c = 0; c < Cols; c++)
                    if (City[r, c] == letter)
                        return (Rows - r, c + 1); // convert to 1-based, bottom-origin
            return (0, 0);
        }

        private static char LetterAt(int row, int col)
        {
            // Convert bottom-origin 1..4 to internal 0..3 with row 0 at top
            int rr = Rows - row;
            int cc = col - 1;
            if (rr < 0 || rr >= Rows || cc < 0 || cc >= Cols) return '?';
            return City[rr, cc];
        }

        private static void PrintMap()
        {
            Console.WriteLine();
            Console.WriteLine("MAP OF THE CITY OF HYATTSVILLE");
            Console.WriteLine("   +--1----2----3----4--+   (columns)");
            for (int r = 0; r < Rows; r++)
            {
                int shownRow = Rows - r; // bottom-origin label
                Console.Write($" {shownRow} | ");
                for (int c = 0; c < Cols; c++)
                {
                    Console.Write($"{City[r, c]}    ");
                }
                Console.WriteLine("| " + shownRow);
            }
            Console.WriteLine("   +--1----2----3----4--+");
            Console.WriteLine("        (columns)\n");
        }

        private static void PrintHelp()
        {
            Console.WriteLine("\nDIRECTIONS:");
            Console.WriteLine("- Houses are labelled A..P across a 4×4 grid.");
            Console.WriteLine("- Use coordinates as ROW,COL with numbers 1..4.");
            Console.WriteLine("- Row 1 is the BOTTOM; Column 1 is the LEFT edge.");
            Console.WriteLine("- Example: If the map shows 'A' in the bottom-left, type 1,1.");
            Console.WriteLine("Commands: MAP (show map), HELP (these notes), QUIT (exit)\n");
        }

        private static bool TryParseRowCol(string s, out int row, out int col)
        {
            row = col = 0;
            var parts = s.Split(new[] { ',', ' ', ';', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2) return false;

            if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out row)) return false;
            if (!int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out col)) return false;

            return row >= 1 && row <= Rows && col >= 1 && col <= Cols;
        }

        private static string ReadLineNonEmpty(string prompt)
        {
            Console.Write(prompt);
            string s = Console.ReadLine() ?? "";
            while (string.IsNullOrWhiteSpace(s))
            {
                Console.Write(prompt);
                s = Console.ReadLine() ?? "";
            }
            return s.Trim();
        }

        private static void Append(string line)
        {
            using var w = File.AppendText(LogPath);
            w.WriteLine(line);
        }
    }
}
