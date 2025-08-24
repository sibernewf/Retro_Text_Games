using System;
using System.Collections.Generic;
using System.Linq;

namespace Bulleye
{
    enum ThrowType { FastOverarm = 1, ControlledOverarm = 2, Underarm = 3 }

    sealed class Player
    {
        public string Name { get; }
        public int Score { get; set; }
        public ThrowType? LastThrow { get; set; }
        public Player(string name) { Name = name; }
    }

    static class Program
    {
        static readonly Random Rng = new();

        // ----- Probability tables -----
        // Each table is a list of (score, probability) that must sum to 1.0
        static readonly (int pts, double p)[] Throw1 = // Fast overarm: bullseye or miss
        {
            (40, 0.20), // tweakable
            ( 0, 0.80),
        };

        static readonly (int pts, double p)[] Throw2 = // Controlled overarm: 10/20/30 (+ rare miss)
        {
            (30, 0.25), // tweakable
            (20, 0.35),
            (10, 0.25),
            ( 0, 0.15),
        };

        static readonly (int pts, double p)[] Throw3 = // Underarm (from the booklet; E[S]=18)
        {
            (40, 0.05),
            (30, 0.20),
            (20, 0.30),
            (10, 0.40),
            ( 0, 0.05),
        };

        static readonly Dictionary<ThrowType, (int pts, double p)[]> ProbTables = new()
        {
            { ThrowType.FastOverarm,      Throw1 },
            { ThrowType.ControlledOverarm,Throw2 },
            { ThrowType.Underarm,         Throw3 },
        };

        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("GAME OF BULLSEYE");
            Console.WriteLine("Up to 20 players throw darts at a target with 10, 20, 30, and 40 point zones.");
            Console.WriteLine("The objective is 200 points. Type Q anytime to quit.");
            Console.WriteLine("Throw styles: 1=Fast overarm, 2=Controlled overarm, 3=Underarm.");
            Console.WriteLine("(Press ENTER to repeat your last throw.)\n");

            int n = AskInt("How many players (1–20)? ", 1, 20);
            var players = new List<Player>(n);
            for (int i = 1; i <= n; i++)
            {
                Console.Write($"Name of player {i}? ");
                var raw = Console.ReadLine();
                if (raw != null && raw.Trim().Equals("Q", StringComparison.OrdinalIgnoreCase)) return;
                string name = string.IsNullOrWhiteSpace(raw) ? $"Player {i}" : raw!.Trim();
                players.Add(new Player(name));
            }

            Console.WriteLine();
            PrintThrowMenu();
            Console.WriteLine();

            int round = 0;
            bool finished = false;

            while (!finished)
            {
                round++;
                Console.WriteLine($"\nROUND {round}");

                foreach (var p in players)
                {
                    // Ask for throw
                    ThrowType t = AskThrow(p);

                    // Sample a score
                    int pts = SampleScore(ProbTables[t]);

                    // Commentary
                    AnnounceThrow(p, t, pts);

                    // Update
                    p.Score += pts;
                    Console.WriteLine($"TOTAL SCORE = {p.Score}");

                    if (p.Score >= 200) finished = true;
                }

                // If at least one player hit 200, announce winners (ties allowed)
                if (finished)
                {
                    int max = players.Max(x => x.Score);
                    var winners = players.Where(x => x.Score >= 200 && x.Score == max).ToList();

                    Console.WriteLine();
                    if (winners.Count == 1)
                        Console.WriteLine($"{winners[0].Name.ToUpper()} HAS A WINNER!!");
                    else
                        Console.WriteLine($"WE HAVE WINNERS: {string.Join(", ", winners.Select(w => w.Name))}!");

                    Console.WriteLine();
                    foreach (var pl in players.OrderByDescending(x => x.Score))
                        Console.WriteLine($"{pl.Name} scored {pl.Score} points.");

                    Console.WriteLine("\nTHANKS FOR THE GAME!");
                }
            }
        }

        static void PrintThrowMenu()
        {
            Console.WriteLine("THROW   DESCRIPTION           PROBABLE SCORE");
            Console.WriteLine("1       FAST OVERARM          BULLSEYE OR COMPLETE MISS");
            Console.WriteLine("2       CONTROLLED OVERARM    10, 20, OR 30 POINTS (OCCASIONAL MISS)");
            Console.WriteLine("3       UNDERARM              ANYTHING (0,10,20,30,40 — E[S]=18)");
        }

        static ThrowType AskThrow(Player p)
        {
            while (true)
            {
                Console.Write($"{p.Name}'s throw? (1/2/3, ENTER repeats{(p.LastThrow is null ? "" : $" {((int)p.LastThrow).ToString()}")}) ");
                var raw = Console.ReadLine();
                string s = (raw ?? "").Trim().ToUpperInvariant();
                if (s == "Q") Environment.Exit(0);
                if (s == "" && p.LastThrow is not null) return p.LastThrow!.Value;

                if (int.TryParse(s, out int v) && v is >= 1 and <= 3)
                {
                    p.LastThrow = (ThrowType)v;
                    return (ThrowType)v;
                }
            }
        }

        static int SampleScore((int pts, double p)[] table)
        {
            double r = Rng.NextDouble();
            double acc = 0.0;
            foreach (var (pts, prob) in table)
            {
                acc += prob;
                if (r <= acc) return pts;
            }
            // numerical edge
            return table[^1].pts;
        }

        static void AnnounceThrow(Player p, ThrowType t, int pts)
        {
            string desc = t switch
            {
                ThrowType.FastOverarm => "FAST OVERARM",
                ThrowType.ControlledOverarm => "CONTROLLED OVERARM",
                _ => "UNDERARM",
            };

            Console.WriteLine($"{p.Name}'s throw: {desc}");

            if (pts == 40) Console.WriteLine("BULLSEYE!! 40 POINTS!");
            else if (pts == 30) Console.WriteLine("30-POINT ZONE!");
            else if (pts == 20) Console.WriteLine("20-POINT ZONE!");
            else if (pts == 10) Console.WriteLine("10-POINT ZONE!");
            else Console.WriteLine("MISSED THE TARGET. TOO BAD!");
        }

        // ------ helpers ------
        static int AskInt(string prompt, int lo, int hi)
        {
            while (true)
            {
                Console.Write(prompt);
                var raw = Console.ReadLine();
                if ((raw ?? "").Trim().Equals("Q", StringComparison.OrdinalIgnoreCase)) Environment.Exit(0);
                if (int.TryParse(raw, out int v) && v >= lo && v <= hi) return v;
            }
        }
    }
}
