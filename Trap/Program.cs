using System;
using System.Globalization;

namespace TrapGame
{
    internal static class Program
    {
        // Change these to match the BASIC lines 10 & 20 if you like
        private const int MaxGuesses = 6;   // how many guesses you get
        private const int UpperLimit = 100; // secret is between 1 and this number (inclusive)

        private static readonly CultureInfo Us = CultureInfo.GetCultureInfo("en-US");
        private static readonly Random Rng = new Random();

        static void Main()
        {
            Console.WriteLine("TRAP    (Time to trap a mystery number!)\n");

            if (AskYesNo("WANT INSTRUCTIONS (1 FOR YES)? "))
                PrintInstructions();

            do
            {
                PlayOneRound();
            } while (AskYesNo("\nPLAY AGAIN (YES=1, NO=0)? "));
        }

        private static void PlayOneRound()
        {
            // Secret can be integer, but we’ll accept decimal guesses too (like the sample run)
            int secret = Rng.Next(1, UpperLimit + 1);

            Console.WriteLine($"\nI AM THINKING OF A NUMBER BETWEEN 1 AND {UpperLimit}");
            Console.WriteLine($"TRY TO GUESS MY NUMBER. ON EACH GUESS,");
            Console.WriteLine("YOU ENTER 2 NUMBERS, TRYING TO TRAP MY NUMBER BETWEEN THEM.");
            Console.WriteLine("IF YOU WANT TO GUESS ONE SINGLE NUMBER, TYPE THE SAME NUMBER TWICE.");
            Console.WriteLine($"YOU GET {MaxGuesses} GUESSES TO GET MY NUMBER.\n");

            for (int g = 1; g <= MaxGuesses; g++)
            {
                Console.WriteLine($"GUESS #{g}");
                double a = AskDouble("   First trap number: ");
                double b = AskDouble("   Second trap number: ");

                // Normalize
                double low = Math.Min(a, b);
                double high = Math.Max(a, b);

                // Exact-guess rule: both equal the same value and match the secret
                if (Math.Abs(a - b) < 1e-12 && Math.Abs(a - secret) < 1e-12)
                {
                    Console.WriteLine("YOU GOT IT!!!");
                    return;
                }

                // Guidance
                if (high < secret)
                {
                    Console.WriteLine("MY NUMBER IS LARGER THAN YOUR TRAP NUMBERS.");
                }
                else if (low > secret)
                {
                    Console.WriteLine("MY NUMBER IS SMALLER THAN YOUR TRAP NUMBERS.");
                }
                else
                {
                    Console.WriteLine("YOU HAVE TRAPPED MY NUMBER.");
                }

                if (g < MaxGuesses) Console.WriteLine();
            }

            Console.WriteLine($"\nSORRY, THAT'S {MaxGuesses} GUESSES. NUMBER WAS {secret}.");
        }

        private static void PrintInstructions()
        {
            Console.WriteLine();
            Console.WriteLine("I THINK OF A SECRET NUMBER.");
            Console.WriteLine("ON EACH GUESS YOU ENTER TWO NUMBERS AS A RANGE.");
            Console.WriteLine("I'LL TELL YOU IF MY NUMBER IS LARGER, SMALLER, OR TRAPPED");
            Console.WriteLine("BETWEEN YOUR TWO NUMBERS. TO WIN, ENTER MY NUMBER AS BOTH");
            Console.WriteLine("TRAP NUMBERS (E.G., 42 and 42).");
            Console.WriteLine();
        }

        private static bool AskYesNo(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = (Console.ReadLine() ?? "").Trim();
                if (s == "1" || s.Equals("YES", StringComparison.OrdinalIgnoreCase)) return true;
                if (s == "0" || s.Equals("NO",  StringComparison.OrdinalIgnoreCase)) return false;
            }
        }

        private static double AskDouble(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = Console.ReadLine();
                if (double.TryParse(s, NumberStyles.Float, Us, out double v)) return v;
                Console.WriteLine("PLEASE ENTER A NUMBER.");
            }
        }
    }
}
