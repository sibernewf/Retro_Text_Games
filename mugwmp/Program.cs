using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MUGWMP
{
    internal static class Program
    {
        private static readonly Random Rng = new Random();

        private const int GridSize = 10;     // coordinates 0..9
        private const int Mugwumps = 4;      // number hidden
        private const int MaxGuesses = 10;   // allowed guesses

        private sealed class Point
        {
            public int X;
            public int Y;
            public bool Found;
            public override string ToString() => $"({X},{Y})";
        }

        static void Main()
        {
            Console.Title = "MUGWMP — Find 4 Mugwumps";
            Console.WriteLine("MUGWMP — FIND 4 MUGWUMPS IN HIDING\n");

            do
            {
                PlayOneGame();
            }
            while (AskYesNo("\nTHAT WAS FUN!  LET'S PLAY AGAIN... (Y/N) "));
        }

        private static void PlayOneGame()
        {
            // Place 4 unique mugwumps
            var mugs = new List<Point>();
            while (mugs.Count < Mugwumps)
            {
                int x = Rng.Next(GridSize);
                int y = Rng.Next(GridSize);
                if (!mugs.Any(p => p.X == x && p.Y == y))
                    mugs.Add(new Point { X = x, Y = y, Found = false });
            }

            Console.WriteLine(
@"THE OBJECT OF THIS GAME IS TO FIND FOUR MUGWUMPS
HIDDEN ON A 10 BY 10 GRID. HOMEBASE IS POSITION 0,0.
EACH GUESS IS TWO NUMBERS (0..9), ""X,Y"" — X IS TO THE RIGHT OF HOMEBASE,
Y IS ABOVE HOMEBASE. YOU GET 10 TRIES. AFTER EACH TRY, I WILL TELL
YOU HOW FAR YOU ARE FROM EACH MUGWUMP.

Type HELP to see these rules again. Type QUIT to give up.
");

            int found = 0;

            for (int turn = 1; turn <= MaxGuesses; turn++)
            {
                // Get a guess
                (int gx, int gy)? guess = ReadGuess($"TURN NO. {turn}  WHAT IS YOUR GUESS? ");
                if (guess == null)
                {
                    // user typed QUIT
                    Reveal(mugs);
                    return;
                }

                int x = guess.Value.gx;
                int y = guess.Value.gy;

                // Evaluate
                bool foundAnyThisTurn = false;
                for (int i = 0; i < mugs.Count; i++)
                {
                    var m = mugs[i];
                    if (m.Found) continue;

                    double d = Distance(x, y, m.X, m.Y);

                    if (d == 0)
                    {
                        m.Found = true;
                        found++;
                        Console.WriteLine($"YOU HAVE FOUND MUGWUMP {i + 1}");
                        foundAnyThisTurn = true;
                    }
                }

                if (!foundAnyThisTurn)
                {
                    // Print distances for each mugwump still hiding
                    for (int i = 0; i < mugs.Count; i++)
                    {
                        var m = mugs[i];
                        if (m.Found) continue;
                        double d = Distance(x, y, m.X, m.Y);
                        Console.WriteLine($"YOU ARE {d.ToString("0.0", CultureInfo.InvariantCulture)} UNITS FROM MUGWUMP {i + 1}");
                    }
                }

                if (found == Mugwumps)
                {
                    Console.WriteLine($"\nYOU GOT THEM ALL IN {turn} TURN{(turn == 1 ? "" : "S")}!");
                    return;
                }
            }

            // Out of turns — reveal locations
            Console.WriteLine("\nSORRY, THAT'S 10 TRIES.  HERE IS WHERE THEY'RE HIDING:");
            Reveal(mugs);
        }

        private static void Reveal(IEnumerable<Point> mugs)
        {
            int i = 1;
            foreach (var m in mugs)
            {
                Console.WriteLine($"MUGWUMP {i} IS AT {m}");
                i++;
            }
        }

        private static double Distance(int x1, int y1, int x2, int y2)
        {
            double dx = x1 - x2;
            double dy = y1 - y2;
            // exact 0 check to match “FOUND” cases
            if (dx == 0 && dy == 0) return 0;
            double d = Math.Sqrt(dx * dx + dy * dy);
            // match old BASIC one-decimal style
            return Math.Round(d, 1, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Reads a guess "x,y" where x,y ∈ [0,9].
        /// Returns null if user typed QUIT.
        /// Prints HELP text if requested and re-prompts.
        /// </summary>
        private static (int x, int y)? ReadGuess(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string line = (Console.ReadLine() ?? "").Trim();

                if (line.Equals("QUIT", StringComparison.OrdinalIgnoreCase))
                    return null;

                if (line.Equals("HELP", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("ENTER TWO INTEGERS 0..9 AS X,Y — EXAMPLE: 4,7");
                    continue;
                }

                // Accept formats: "x,y" or "x y"
                line = line.Replace(" ", "");
                var parts = line.Split(',');
                if (parts.Length == 2 &&
                    int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int x) &&
                    int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int y) &&
                    x >= 0 && x < GridSize && y >= 0 && y < GridSize)
                {
                    return (x, y);
                }

                Console.WriteLine("PLEASE ENTER A VALID GUESS, e.g. 5,3 (both numbers 0..9), or type HELP.");
            }
        }

        private static bool AskYesNo(string prompt)
        {
            Console.Write(prompt);
            var s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
            return s.StartsWith("Y");
        }
    }
}
