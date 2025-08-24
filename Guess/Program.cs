using System;
using System.Globalization;

namespace GuessGame
{
    internal static class Program
    {
        static void Main()
        {
            Console.Title = "GUESS — Guess a Random Number";
            new Guess().Run();
        }
    }

    internal sealed class Guess
    {
        readonly Random rng = new();

        public void Run()
        {
            PrintIntro();

            while (true)
            {
                int limit = ReadInt("SET THE UPPER LIMIT (>= 1): ", min: 1, max: int.MaxValue, allowQuit: true);
                if (limit == int.MinValue) return;

                int target = rng.Next(limit + 1); // 0..limit inclusive
                int low = 0, high = limit;
                int guesses = 0;

                // Optimal bound using binary search on (limit+1) possibilities
                int optimal = (int)Math.Ceiling(Math.Log2((double)limit + 1.0));

                Console.WriteLine();
                Console.WriteLine($"I'M THINKING OF A NUMBER BETWEEN {low} AND {high} (INCLUSIVE).");
                Console.WriteLine($"HINT: An optimal strategy (binary search) needs ≤ {optimal} guesses.\n");

                while (true)
                {
                    int g = ReadInt($"YOUR GUESS ({low}..{high})? ", low, high, allowQuit: true);
                    if (g == int.MinValue) return;

                    guesses++;

                    if (g == target)
                    {
                        Console.WriteLine($"CORRECT! The number was {target}.");
                        Console.WriteLine(ResultLine(guesses, optimal));
                        Console.WriteLine();
                        if (!AskYesNo("PLAY AGAIN (Y/N)? ")) return;
                        Console.WriteLine();
                        break;
                    }

                    if (g < target)
                    {
                        Console.WriteLine("TOO LOW.");
                        if (g + 1 > low) low = g + 1;
                    }
                    else
                    {
                        Console.WriteLine("TOO HIGH.");
                        if (g - 1 < high) high = g - 1;
                    }
                }
            }
        }

        static string ResultLine(int guesses, int optimal)
        {
            if (guesses < optimal) return $"Impressive—{guesses} guesses (better than the usual {optimal}).";
            if (guesses == optimal) return $"Nice! {guesses} guesses—right at the optimal bound.";
            return $"{guesses} guesses. With binary search you can usually do it in {optimal}.";
        }

        // ---------------- I/O helpers ----------------
        static int ReadInt(string prompt, int min, int max, bool allowQuit = false)
        {
            while (true)
            {
                Console.Write(prompt);
                string? s = Console.ReadLine();
                if (s == null) continue;
                s = s.Trim();
                if (allowQuit && s.Equals("Q", StringComparison.OrdinalIgnoreCase)) return int.MinValue;

                if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out int n) && n >= min && n <= max)
                    return n;

                Console.WriteLine($"Enter an integer from {min} to {max}{(allowQuit ? " (or Q to quit)" : "")}.");
            }
        }

        static bool AskYesNo(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
                if (s is "Y" or "YES") return true;
                if (s is "N" or "NO") return false;
            }
        }

        static void PrintIntro()
        {
            Console.WriteLine("GUESS — Guess a Random Number");
            Console.WriteLine("I will choose an integer N in [0..LIMIT] that you set. You guess, I say HIGH/LOW.");
            Console.WriteLine("Tip: Binary search finds it in about log2(LIMIT+1) guesses.");
            Console.WriteLine("Type Q at any prompt to quit.\n");
        }
    }
}
