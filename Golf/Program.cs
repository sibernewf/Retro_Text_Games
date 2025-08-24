using System;
using System.Collections.Generic;
using System.Globalization;

namespace Golf18
{
    internal static class Program
    {
        static void Main()
        {
            Console.Title = "GOLF — 18 Holes (BASIC-style multi-player)";
            new GolfGame().Run();
        }
    }

    internal sealed class GolfGame
    {
        readonly Random rng = new();

        // A friendly default course (lengths in yards, typical pars).
        // You can tweak these; max 18.
        private readonly (int yards, int par)[] course =
        {
            (356,4), (325,4), (202,3), (405,4), (415,4), (195,3),
            (512,5), (380,4), (538,5), // front 9
            (331,4), (164,3), (438,4), (410,4), (525,5), (398,4),
            (214,3), (455,4), (545,5)  // back 9
        };

        // Lie state
        enum Lie { Fairway, Rough, Trap, Water, Green }

        // Clubs 1–9 (roughly mirrors the listing text)
        private readonly Dictionary<int, Club> clubs = new()
        {
            {1, new Club("Driver",     180, 300, Lie.Fairway)},
            {2, new Club("3-Wood",     160, 240, Lie.Fairway)},
            {3, new Club("5-Wood",     140, 220, Lie.Fairway)},
            {4, new Club("3-Iron",     120, 200, Lie.Fairway)},
            {5, new Club("5-Iron",     100, 170, Lie.Fairway)},
            {6, new Club("7-Iron",      80, 150, Lie.Rough)},     // good from rough
            {7, new Club("9-Iron",      60, 120, Lie.Trap)},      // good from trap
            {8, new Club("Wedge",       20,  70, Lie.Trap)},      // good from trap
            {9, new Club("Putter",       1,  30, Lie.Green)}      // green only
        };

        public void Run()
        {
            PrintIntro();

            // players
            int players = ReadInt("HOW MANY PLAYERS ARE PLAYING TODAY? ", 1, 4, allowQuit: true);
            if (players == int.MinValue) return;

            int maxHoles = Math.Min(course.Length, 18);
            int holesToPlay = ReadInt($"HOW MANY HOLES (UP TO {maxHoles}) DO YOU WANT TO PLAY? ", 1, maxHoles, allowQuit: true);
            if (holesToPlay == int.MinValue) return;

            var names = new string[players];
            for (int i = 0; i < players; i++)
            {
                Console.Write($"PLAYER {i+1} NAME (ENTER FOR P{i+1}): ");
                var s = Console.ReadLine();
                names[i] = string.IsNullOrWhiteSpace(s) ? $"PLAYER {i+1}" : s.Trim();
            }
            Console.WriteLine();

            var totals = new int[players];
            var vsParTotals = new int[players];

            for (int h = 0; h < holesToPlay; h++)
            {
                int yards = course[h].yards;
                int par = course[h].par;
                Console.WriteLine($"HOLE {h+1} IS {yards} YARDS — PAR {par}");
                Console.WriteLine();

                for (int p = 0; p < players; p++)
                {
                    int shots = PlayHole(names[p], yards, par);
                    totals[p] += shots;
                    vsParTotals[p] += shots - par;
                    Console.WriteLine($"{names[p]} SCORED {shots} ON HOLE {h+1} (TOTAL {totals[p]}, VS PAR {Signed(vsParTotals[p])})");
                    Console.WriteLine();
                }
            }

            Console.WriteLine("— ROUND COMPLETE —");
            for (int p = 0; p < players; p++)
                Console.WriteLine($"{names[p]}: {totals[p]} strokes, {Signed(vsParTotals[p])} vs par");
        }

        int PlayHole(string player, int holeYards, int par)
        {
            int remaining = holeYards;
            Lie lie = Lie.Fairway;
            int strokes = 0;

            while (true)
            {
                if (remaining <= 30) lie = Lie.Green;

                Console.WriteLine($"{player}, DISTANCE REMAINING TO PIN IS {remaining} YARDS.");
                Console.WriteLine(lie switch
                {
                    Lie.Fairway => "YOU ARE IN THE FAIRWAY.",
                    Lie.Rough   => "YOU ARE IN THE ROUGH.",
                    Lie.Trap    => "YOU ARE IN A SAND TRAP.",
                    Lie.Water   => "YOU ARE DROPPED NEAR WATER (FAIRWAY LIE).",
                    Lie.Green   => "YOU ARE ON THE GREEN.",
                    _ => ""
                });

                int clubNum = ReadClub(lie);
                if (clubNum == int.MinValue) { Environment.Exit(0); }
                Club club = clubs[clubNum];
                strokes++;

                // Duff chance (not on putts)
                if (clubNum != 9 && rng.Next(100) < 5)
                {
                    Console.WriteLine("YOU DUFFED THE SHOT! BALL TRICKLES A FEW YARDS.");
                    int duff = rng.Next(0, 6);
                    remaining = Math.Max(1, remaining - duff);
                    // lie doesn’t change, continue
                    continue;
                }

                // Compute carry
                int carry = ShotDistance(club, lie, remaining);

                // On green & putting
                if (lie == Lie.Green && clubNum == 9)
                {
                    bool sunk = ResolvePutt(ref remaining, carry);
                    Console.WriteLine(sunk ? "HOLE OUT!" : $"PUTT ROLLS {carry} YARDS. {remaining} LEFT.");
                    if (sunk) break;
                    continue;
                }

                // Full shot results
                remaining = Math.Max(1, remaining - carry);
                Console.WriteLine($"DISTANCE OF SHOT IS {carry} YARDS.");
                if (remaining <= 30) Console.WriteLine($"{player} IS ON THE GREEN — CHOOSE YOUR CLUB.");

                // Hazards (only when not on green)
                if (remaining > 30)
                {
                    // water is rare; penalty stroke and drop 20 yards back
                    if (rng.Next(100) < 3)
                    {
                        Console.WriteLine("SPLASH! YOU FOUND WATER — PENALTY STROKE AND DROP.");
                        strokes++; // penalty
                        remaining = Math.Min(holeYards, remaining + 20);
                        lie = Lie.Fairway;
                        continue;
                    }

                    // trap vs rough
                    int hazardRoll = rng.Next(100);
                    if (hazardRoll < 6)
                    {
                        lie = Lie.Trap;
                        Console.WriteLine("IN TRAP.");
                    }
                    else if (hazardRoll < 16)
                    {
                        lie = Lie.Rough;
                        Console.WriteLine("IN ROUGH.");
                    }
                    else lie = Lie.Fairway;
                }
            }

            Console.WriteLine($"{PuttsWord(strokes)}");
            return strokes;
        }

        int ShotDistance(Club club, Lie lie, int remaining)
        {
            // Base random within club range
            int baseShot = rng.Next(club.Min, club.Max + 1);

            // Lie modifiers
            if (lie == Lie.Rough && club.BestLie != Lie.Rough)
                baseShot = (int)(baseShot * 0.6); // only rough-friendly clubs fly well
            if (lie == Lie.Trap && club.BestLie != Lie.Trap)
                baseShot = rng.Next(0, 21); // really hard to escape trap with a bad club

            // Clip: don’t fly way over when very close (except chips)
            if (remaining < 100 && club.Max > 140)
                baseShot = Math.Min(baseShot, (int)(remaining * 0.9));

            // Tiny chance of a “perfect” shot when remaining ≤ 220 and using a wood/long iron
            if (remaining <= 220 && (club.Name.Contains("Wood") || club.Name.Contains("3-Iron")) && rng.Next(600) == 0)
            {
                Console.WriteLine("INCREDIBLE SHOT! RIGHT AT THE FLAG!");
                return remaining; // hole-in-one (or jar from fairway)
            }

            return Math.Max(1, baseShot);
        }

        bool ResolvePutt(ref int remaining, int puttRoll)
        {
            // Model: a putt tries to travel exactly the remaining distance plus/sans a jitter.
            int target = remaining;
            int jitter = rng.Next(-3, 4); // ±3 yards tolerance
            int roll = Math.Max(1, target + jitter);
            remaining = Math.Abs(target - roll);

            // Sink if we rolled within 1 yard past/short
            return remaining <= 1;
        }

        int ReadClub(Lie lie)
        {
            while (true)
            {
                PrintClubs(lie);
                Console.Write("CHOOSE YOUR CLUB (1-9, or Q to quit): ");
                var s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
                if (s == "Q") return int.MinValue;
                if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out int n)
                    && n >= 1 && n <= 9)
                {
                    if (lie == Lie.Green && n != 9)
                    {
                        Console.WriteLine("YOU ARE ON THE GREEN — USE YOUR PUTTER (9).");
                        continue;
                    }
                    if (lie != Lie.Green && n == 9)
                    {
                        Console.WriteLine("YOU ARE NOT ON THE GREEN — CHOOSE ANOTHER CLUB.");
                        continue;
                    }
                    return n;
                }
                Console.WriteLine("INVALID CLUB NUMBER.");
            }
        }

        void PrintIntro()
        {
            Console.WriteLine("DO YOU WANT DIRECTIONS? (Y/N)");
            var a = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
            if (a is "Y" or "YES")
            {
                Console.WriteLine();
                Console.WriteLine("You have a choice of 9 clubs. On each shot choose a club; the");
                Console.WriteLine("computer figures the distance and whether you end in rough, trap,");
                Console.WriteLine("or water. On the green you must use the putter. Up to 4 players");
                Console.WriteLine("can play up to 18 holes. Enter 'Q' at any prompt to quit.");
                Console.WriteLine();
                PrintClubs(Lie.Fairway);
                Console.WriteLine();
            }
        }

        void PrintClubs(Lie context)
        {
            Console.WriteLine("CLUBS:");
            foreach (var kv in clubs)
            {
                var c = kv.Value;
                string tip = c.BestLie switch
                {
                    Lie.Rough => " (good from rough)",
                    Lie.Trap => " (good from trap)",
                    Lie.Green => " (use on green)",
                    _ => ""
                };
                if (kv.Key == 9)
                    Console.WriteLine($"  {kv.Key}: {c.Name,-8} range {c.Min}-{c.Max} yds{tip}");
                else
                    Console.WriteLine($"  {kv.Key}: {c.Name,-8} range {c.Min}-{c.Max} yds{tip}");
            }
        }

        static string PuttsWord(int strokes)
        {
            return strokes switch
            {
                1 => "HOLE IN ONE!!",
                2 => "EAGLE!",
                3 => "BIRDIE/3 PUTTS TOTAL",
                _ => $"{strokes} STROKES"
            };
        }

        static string Signed(int n) => n >= 0 ? $"+{n}" : n.ToString();

        int ReadInt(string prompt, int min, int max, bool allowQuit = false)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
                if (allowQuit && s == "Q") return int.MinValue;
                if (int.TryParse(s, out var x) && x >= min && x <= max) return x;
                Console.WriteLine($"ENTER A NUMBER FROM {min} TO {max}{(allowQuit ? " (or Q to quit)" : "")}.");
            }
        }

        sealed class Club
        {
            public string Name { get; }
            public int Min { get; }
            public int Max { get; }
            public Lie BestLie { get; }
            public Club(string name, int min, int max, Lie best) { Name = name; Min = min; Max = max; BestLie = best; }
        }
    }
}
