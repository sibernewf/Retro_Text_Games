using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BattleModern
{
    class Cell
    {
        public int R;
        public int C;
        public Cell(int r, int c) { R = r; C = c; }
    }

    class Ship
    {
        public int Id;
        public string Class = string.Empty; // ✅ Default value to prevent warning
        public int Size;
        public List<Cell> Cells = new();
        public HashSet<(int r, int c)> Hits = new();

        public bool Occupies(int r, int c) => Cells.Any(t => t.R == r && t.C == c);
        public bool IsSunk => Hits.Count >= Size;
        public void Hit(int r, int c) => Hits.Add((r, c));
    }

    class Board
    {
        public const int N = 10;
        private readonly Random rng = new();
        public List<Ship> Ships = new();
        public HashSet<(int r, int c)> Shots = new();

        public Board()
        {
            // Fleet: Destroyers(2) x4, Cruisers(3) x3, Carriers(5) x2
            AddFleet(("Destroyer", 2, 4), ("Cruiser", 3, 3), ("Carrier", 5, 2));
        }

        private void AddFleet(params (string cls, int size, int count)[] defs)
        {
            int id = 1;
            foreach (var (cls, size, count) in defs)
            {
                for (int i = 0; i < count; i++)
                {
                    var ship = new Ship { Id = id++, Class = cls, Size = size };
                    PlaceShip(ship);
                    Ships.Add(ship);
                }
            }
        }

        private void PlaceShip(Ship s)
        {
            while (true)
            {
                bool horiz = rng.Next(2) == 0;
                int r = rng.Next(1, N + 1);
                int c = rng.Next(1, N + 1);
                if (horiz) c = rng.Next(1, N - s.Size + 2);
                else r = rng.Next(1, N - s.Size + 2);

                var cells = new List<Cell>();
                bool clear = true;
                for (int k = 0; k < s.Size; k++)
                {
                    int rr = r + (horiz ? 0 : k);
                    int cc = c + (horiz ? k : 0);
                    if (Ships.Any(other => other.Occupies(rr, cc))) { clear = false; break; }
                    cells.Add(new Cell(rr, cc));
                }
                if (!clear) continue;
                s.Cells = cells;
                return;
            }
        }

        public (bool valid, string msg, string outcome) Fire(int r, int c, out Ship? shipHit)
        {
            shipHit = null;

            if (r < 1 || r > N || c < 1 || c > N)
                return (false, "INVALID INPUT. TRY AGAIN.", "invalid");

            if (Shots.Contains((r, c)))
                return (true, "YOU HAVE ALREADY PUT A HOLE IN THE OCEAN AT THAT POINT. SPLASH! TRY AGAIN.", "repeat");

            Shots.Add((r, c));

            foreach (var s in Ships)
            {
                if (s.Occupies(r, c))
                {
                    s.Hit(r, c);
                    shipHit = s;
                    if (s.IsSunk)
                        return (true, $"A DIRECT HIT ON SHIP NUMBER {s.Id} — AND YOU SINK IT. HURRAH FOR THE GOOD GUYS.", "sunk");
                    else
                        return (true, $"A DIRECT HIT ON SHIP NUMBER {s.Id}.", "hit");
                }
            }

            return (true, "SPLASH! TRY AGAIN.", "miss");
        }

        public bool AllSunk => Ships.All(s => s.IsSunk);

        public (int d, int c, int a) Losses()
        {
            int d = Ships.Count(s => s.Class == "Destroyer" && s.IsSunk);
            int c = Ships.Count(s => s.Class == "Cruiser" && s.IsSunk);
            int a = Ships.Count(s => s.Class == "Carrier" && s.IsSunk);
            return (d, c, a);
        }
    }

    class Game
    {
        private readonly Board board = new();
        private readonly List<string> log = new();
        private int shots = 0, hits = 0;

        public void Run()
        {
            Intro();

            while (true)
            {
                var (r, c, quit) = PromptShot();
                if (quit) break;

                var (ok, msg, outcome) = board.Fire(r, c, out var ship);
                if (!ok) { Log(msg); continue; }

                shots++;
                if (outcome == "hit" || outcome == "sunk") hits++;

                Log(msg);
                ShowTally();

                if (outcome == "sunk")
                {
                    var (d, cr, ca) = board.Losses();
                    Log($"SO FAR THE BAD GUYS HAVE LOST {d} DESTROYER(S), {cr} CRUISER(S) AND {ca} AIRCRAFT CARRIER(S).");
                }

                if (board.AllSunk)
                {
                    Log("\n************************");
                    Log("CONGRATULATIONS — YOU SANK THEIR ENTIRE FLEET!");
                    ShowTally(final: true);
                    Log("************************");
                    break;
                }
            }

            File.WriteAllLines("battle_playbyplay.txt", log);
            Console.WriteLine($"\nPlay-by-play saved to: {Path.GetFullPath("battle_playbyplay.txt")}");
        }

        private void Intro()
        {
            Log("THIS PROGRAM IS 'BATTLE'.");
            Log("THE FOLLOWING CODE OF THE BAD GUYS' FLEET DISPOSITION HAS BEEN CAPTURED BUT NOT DECODED.");
            Log("DE-CODE IT AND USE IT IF YOU CAN — BUT KEEP THE DECODING METHOD A SECRET.\n");
            Log("START GAME");
            Log("Type coordinates as row,column (e.g., 5,2) or like A7. Q to quit.");
        }

        private (int r, int c, bool quit) PromptShot()
        {
            while (true)
            {
                Console.Write("\nENTER YOUR SHOT (R,C or A1) > ");
                var s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
                if (s == "Q") return (0, 0, true);

                // Format: "r,c"
                if (s.Contains(','))
                {
                    var parts = s.Split(',');
                    if (parts.Length == 2 && int.TryParse(parts[0], out int r) && int.TryParse(parts[1], out int c))
                        return (r, c, false);
                }
                // Format: "A10"
                if (s.Length >= 2 && s.Length <= 3 && char.IsLetter(s[0]))
                {
                    int r = s[0] - 'A' + 1;
                    if (int.TryParse(s.Substring(1), out int c))
                        return (r, c, false);
                }

                Console.WriteLine("Invalid format. Try '5,2' or 'C7' (Q to quit).");
            }
        }

        private void ShowTally(bool final = false)
        {
            double ratio = hits == 0 ? double.PositiveInfinity : (double)(shots - hits) / hits;
            string rstr = double.IsInfinity(ratio) ? "∞" : ratio.ToString("0.###");
            string prefix = final ? "YOUR FINAL SPLASH/HIT RATIO IS" : "YOUR CURRENT SPLASH/HIT RATIO IS";
            Log($"{prefix} {rstr}.");
        }

        private void Log(string s) { Console.WriteLine(s); log.Add(s); }
    }

    class Program
    {
        static void Main()
        {
            Console.WriteLine("*** BATTLE (Modernized) ***");
            Console.WriteLine("Q at any prompt to quit.\n");
            var g = new Game();
            g.Run();
        }
    }
}
