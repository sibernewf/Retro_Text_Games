using System;
using System.Globalization;

namespace NumberGame
{
    internal static class Program
    {
        private static readonly Random Rng = new Random();

        // Scoring table: change per round based on distance |guess - secret|
        // 0 -> +20, 1 -> +10, 2 -> 0, 3 -> -10, 4 -> -20
        private static int ScoreDeltaByDistance(int d) =>
            d switch { 0 => +20, 1 => +10, 2 => 0, 3 => -10, 4 => -20, _ => 0 };

        private const int StartPoints = 100;
        private const int WinPoints = 500;
        private const double JackpotChance = 0.12;  // 12% chance each round

        static void Main()
        {
            Console.Title = "NUMBER — Random Number Game";
            PrintBanner();

            do
            {
                PlayOneGame();
            } while (AskYesNo("\nPLAY AGAIN? (Y/N) "));
        }

        private static void PlayOneGame()
        {
            int points = StartPoints;
            Console.WriteLine($"\nYOU NOW HAVE {points} POINTS");
            Console.WriteLine("BY GUESSING NUMBERS FROM 1 TO 5, YOU CAN GAIN OR LOSE");
            Console.WriteLine("POINTS DEPENDING UPON HOW CLOSE YOU GET TO A RANDOM");
            Console.WriteLine("NUMBER SELECTED BY THE COMPUTER.\n");
            Console.WriteLine("YOU OCCASIONALLY WILL GET A JACKPOT WHICH WILL DOUBLE (!) YOUR POINT COUNT.");
            Console.WriteLine($"YOU WIN WHEN YOU GET {WinPoints} POINTS.\n");

            while (points < WinPoints)
            {
                int guess = ReadInt("GUESS A NUMBER FROM 1 TO 5? ", 1, 5);
                int secret = Rng.Next(1, 6);

                int delta = ScoreDeltaByDistance(Math.Abs(guess - secret));
                points += delta;

                if (delta >= 0)
                    Console.WriteLine($"NICE! SECRET WAS {secret}. YOU GAINED {delta}.");
                else
                    Console.WriteLine($"OOPS — SECRET WAS {secret}. YOU LOST {-delta}.");

                // Jackpot: small probability to double *after* this round’s change
                if (Rng.NextDouble() < JackpotChance)
                {
                    Console.WriteLine("YOU HIT THE JACKPOT!");
                    points *= 2;
                }

                Console.WriteLine($"YOU HAVE {points} POINTS\n");
            }

            Console.WriteLine($"!!! YOU WIN !!! WITH {points} POINTS");
        }

        private static void PrintBanner()
        {
            Console.WriteLine("NUMBER — RANDOM NUMBER GAME\n");
        }

        private static bool AskYesNo(string prompt)
        {
            Console.Write(prompt);
            while (true)
            {
                string s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
                if (s.StartsWith("Y")) return true;
                if (s.StartsWith("N")) return false;
                Console.Write("TYPE YES OR NO: ");
            }
        }

        private static int ReadInt(string prompt, int min, int max)
        {
            while (true)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int v)
                    && v >= min && v <= max)
                    return v;
                Console.WriteLine($"ENTER A WHOLE NUMBER FROM {min} TO {max}.");
            }
        }
    }
}
