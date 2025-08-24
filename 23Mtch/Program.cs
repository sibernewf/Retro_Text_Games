using System;

namespace Matches23
{
    internal static class Program
    {
        private const int StartingMatches = 23;
        private static readonly Random Rng = new Random();

        static void Main()
        {
            Console.WriteLine("23 MATCHES GAME\n");
            Console.WriteLine("Let's play 23 matches. We start with 23 matches.");
            Console.WriteLine("You move first. You may take 1, 2, or 3 matches.");
            Console.WriteLine("Then I move, you move, and so on. The one who takes");
            Console.WriteLine("the last match loses!");
            Console.WriteLine("Good luck and may the best computer (ha ha) win.\n");

            do
            {
                PlayOneGame();
            }
            while (AskYesNo("\nPLAY AGAIN? (YES=1, NO=0) "));
        }

        private static void PlayOneGame()
        {
            int matches = StartingMatches;

            while (matches > 0)
            {
                Console.WriteLine($"\nThere are now {matches} matches.");

                // Player move
                int taken = AskInt("How many do you take? (1–3): ", 1, 3);
                if (taken > matches)
                {
                    Console.WriteLine("YOU CHEATED! But I'll give you another chance.");
                    continue;
                }
                matches -= taken;
                if (matches == 0)
                {
                    Console.WriteLine("You took the last one. Sorry, you lose!");
                    return;
                }

                // Computer move
                int comp = ComputeMove(matches);
                matches -= comp;
                Console.WriteLine($"I took {comp} ... there are now {matches} matches.");
                if (matches == 0)
                {
                    Console.WriteLine("I took the last one. I lose! YOU WIN!!!");
                    return;
                }
            }
        }

        private static int ComputeMove(int matches)
        {
            // Winning strategy: leave multiple of 4 for player
            int rem = matches % 4;
            if (rem == 0)
            {
                // no forced win, pick random legal move
                return Rng.Next(1, Math.Min(3, matches) + 1);
            }
            else
            {
                return rem;
            }
        }

        private static int AskInt(string prompt, int min, int max)
        {
            while (true)
            {
                Console.Write(prompt);
                string? s = Console.ReadLine();
                if (int.TryParse(s, out int v) && v >= min && v <= max)
                    return v;
                Console.WriteLine($"PLEASE ENTER A NUMBER FROM {min} TO {max}.");
            }
        }

        private static bool AskYesNo(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string? s = (Console.ReadLine() ?? "").Trim();
                if (s == "1" || s.Equals("YES", StringComparison.OrdinalIgnoreCase)) return true;
                if (s == "0" || s.Equals("NO",  StringComparison.OrdinalIgnoreCase)) return false;
            }
        }
    }
}
