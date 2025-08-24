using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SalvoGame
{
    class Program
    {
        static void Main()
        {
            Console.Title = "SALVO — Naval Gun Battle";
            var game = new Salvo();
            game.Run();
        }
    }

    enum Cell { Empty, Ship, Hit, Miss }
    enum Dir { H, V, D1, D2 } // horiz, vert, diag down-right, diag up-right

    sealed class ShipType
    {
        public string Name { get; }
        public int Length { get; }
        public int ShotsIfAlive { get; }
        public ShipType(string name, int length, int shots) { Name = name; Length = length; ShotsIfAlive = shots; }
    }

    sealed class Ship
    {
        public ShipType Type { get; }
        public List<(int r, int c)> Cells { get; } = new();
        public HashSet<(int r, int c)> Hits { get; } = new();
        public Ship(ShipType type) { Type = type; }
        public bool IsSunk => Hits.Count >= Cells.Count;
        public bool Contains(int r, int c) => Cells.Contains((r, c));
        public bool RegisterHit(int r, int c)
        {
            if (Contains(r, c)) { Hits.Add((r, c)); return true; }
            return false;
        }
    }

    sealed class Board
    {
        public const int N = 10;
        public Cell[,] Grid { get; } = new Cell[N + 1, N + 1]; // 1-based
        public List<Ship> Fleet { get; } = new();

        public bool InBounds(int r, int c) => r >= 1 && r <= N && c >= 1 && c <= N;

        public bool CanPlace(int r, int c) => InBounds(r, c) && Grid[r, c] == Cell.Empty;

        public bool PlaceShip(Ship ship, (int r, int c) start, (int r, int c) end)
        {
            var cells = TraceLine(start, end);
            if (cells is null || cells.Count != ship.Type.Length) return false;
            // Ensure straight: H, V, or diagonals only
            int dr = Math.Sign(end.r - start.r), dc = Math.Sign(end.c - start.c);
            if (!((dr == 0 && dc != 0) || (dc == 0 && dr != 0) || (Math.Abs(dr) == 1 && Math.Abs(dc) == 1))) return false;

            foreach (var (r, c) in cells)
                if (!CanPlace(r, c)) return false;

            foreach (var (r, c) in cells)
            {
                Grid[r, c] = Cell.Ship;
                ship.Cells.Add((r, c));
            }
            Fleet.Add(ship);
            return true;
        }

        public static List<(int r, int c)>? TraceLine((int r, int c) a, (int r, int c) b)
        {
            int dr = Math.Sign(b.r - a.r);
            int dc = Math.Sign(b.c - a.c);
            if (dr == 0 && dc == 0) return null;

            // only H/V/diag
            if (!((dr == 0 && dc != 0) || (dc == 0 && dr != 0) || (Math.Abs(dr) == 1 && Math.Abs(dc) == 1))) return null;

            var cells = new List<(int r, int c)>() { a };
            int r = a.r, c = a.c;
            while (r != b.r || c != b.c)
            {
                r += dr; c += dc;
                cells.Add((r, c));
            }
            return cells;
        }

        public (bool hit, Ship? ship, bool sunk) FireAt(int r, int c)
        {
            if (!InBounds(r, c)) return (false, null, false);

            if (Grid[r, c] == Cell.Hit || Grid[r, c] == Cell.Miss)
                return (false, null, false); // already fired here; caller should avoid duplicates

            if (Grid[r, c] == Cell.Ship)
            {
                Grid[r, c] = Cell.Hit;
                var ship = Fleet.First(s => s.Contains(r, c));
                ship.RegisterHit(r, c);
                return (true, ship, ship.IsSunk);
            }
            else
            {
                Grid[r, c] = Cell.Miss;
                return (false, null, false);
            }
        }

        public bool AllSunk => Fleet.All(s => s.IsSunk);

        public int ShotsAvailable => Fleet.Where(s => !s.IsSunk).Sum(s => s.Type.ShotsIfAlive);
    }

    sealed class Salvo
    {
        readonly ShipType Battleship = new("BATTLESHIP", 5, 3);
        readonly ShipType Cruiser    = new("CRUISER",     3, 2);
        readonly ShipType DestroyerA = new("DESTROYER(A)",2, 1);
        readonly ShipType DestroyerB = new("DESTROYER(B)",2, 1);

        readonly Random rng = new();

        Board player = new();
        Board enemy  = new();

        HashSet<(int r, int c)> enemyShotsTried = new();

        public void Run()
        {
            PrintIntro();

            // Place ships
            Console.Write("AUTO-PLACE YOUR SHIPS? (YES/NO) ");
            bool auto = ReadYesNo();
            PlaceFleet(player, auto);

            Console.WriteLine("\nPLACING ENEMY FLEET...");
            PlaceFleet(enemy, autoPlace: true);

            // Game loop
            int turn = 1;
            while (true)
            {
                Console.WriteLine($"\n===== TURN {turn} =====");

                if (PlayerTurn()) { Console.WriteLine("\nYOU HAVE WON!"); break; }
                if (EnemyTurn())  { Console.WriteLine("\nYOU HAVE LOST!"); break; }

                turn++;
            }

            Console.WriteLine("\nTHANKS FOR PLAYING.");
        }

        void PrintIntro()
        {
            Console.WriteLine("SALVO — NAVAL GUN BATTLE");
            Console.WriteLine("------------------------");
            Console.WriteLine("10x10 grid, separate boards. Place these ships (H/V/diagonal):");
            Console.WriteLine("  BATTLESHIP (5 squares)  → grants 3 shots while afloat");
            Console.WriteLine("  CRUISER (3 squares)     → grants 2 shots while afloat");
            Console.WriteLine("  DESTROYER(A) (2 squares)→ grants 1 shot while afloat");
            Console.WriteLine("  DESTROYER(B) (2 squares)→ grants 1 shot while afloat");
            Console.WriteLine("Shots per turn = sum of your surviving ships’ shot values.");
            Console.WriteLine("Coordinates are ROW,COL in 1..10, e.g., 4,7\n");
        }

        void PlaceFleet(Board b, bool autoPlace)
        {
            var types = new[] { Battleship, Cruiser, DestroyerA, DestroyerB };
            foreach (var t in types)
            {
                bool placed = false;
                int guard = 0;
                while (!placed && guard++ < 1000)
                {
                    if (autoPlace)
                    {
                        // random start + direction until fits
                        int r = rng.Next(1, Board.N + 1);
                        int c = rng.Next(1, Board.N + 1);
                        var dirs = new (int dr, int dc)[] { (0,1),(1,0),(1,1),(-1,1) }; // H,V,D1,D2
                        dirs = dirs.OrderBy(_ => rng.Next()).ToArray();
                        foreach (var (dr, dc) in dirs)
                        {
                            int r2 = r + (t.Length - 1) * dr;
                            int c2 = c + (t.Length - 1) * dc;
                            if (!b.InBounds(r2, c2)) continue;
                            var ship = new Ship(t);
                            if (b.PlaceShip(ship, (r, c), (r2, c2))) { placed = true; break; }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"\nPlace your {t.Name} (length {t.Length}).");
                        Console.WriteLine("Enter START and END as row,col (1..10) on a straight line (H/V/diagonal).");
                        var start = ReadCoord("  START  (r,c): ");
                        var end   = ReadCoord("  END    (r,c): ");
                        var ship = new Ship(t);
                        if (!b.PlaceShip(ship, start, end))
                        {
                            Console.WriteLine("ILLEGAL PLACEMENT. Try again.");
                            continue;
                        }
                        placed = true;
                    }
                }
                if (!placed) throw new Exception("Failed to place fleet (unexpected).");
            }

            // show player fleet
            if (ReferenceEquals(b, player))
            {
                Console.WriteLine("\nYOUR FLEET IS SET:");
                PrintFleetSummary(b);
            }
        }

        bool PlayerTurn()
        {
            int shots = Math.Max(0, player.ShotsAvailable);
            Console.WriteLine($"\nYOU HAVE {shots} SHOTS");
            var volley = ReadVolley(shots);

            // Resolve player shots
            int hits = 0;
            foreach (var (r, c) in volley)
            {
                var (hit, ship, sunk) = enemy.FireAt(r, c);
                if (hit)
                {
                    hits++;
                    Console.WriteLine($"HIT at {r},{c} → {ship!.Type.Name}{(sunk ? " SUNK!" : "")}");
                }
                else
                {
                    Console.WriteLine($"MISS at {r},{c}");
                }
            }

            if (enemy.AllSunk) return true;
            Console.WriteLine($"(You scored {hits} hit{(hits==1?"":"s")}.)");
            return false;
        }

        bool EnemyTurn()
        {
            int shots = Math.Max(0, enemy.ShotsAvailable);
            Console.WriteLine($"\nENEMY HAS {shots} SHOTS");

            var volley = EnemyVolley(shots);

            int hits = 0;
            foreach (var (r, c) in volley)
            {
                var (hit, ship, sunk) = player.FireAt(r, c);
                if (hit)
                {
                    hits++;
                    Console.WriteLine($"THEY HIT YOUR {ship!.Type.Name} at {r},{c}{(sunk ? " — SUNK!" : "")}");
                }
                else
                {
                    Console.WriteLine($"THEY MISS at {r},{c}");
                }
            }

            if (player.AllSunk) return true;
            Console.WriteLine($"(Enemy scored {hits} hit{(hits==1?"":"s")}.)");
            return false;
        }

        List<(int r, int c)> ReadVolley(int shots)
        {
            var list = new List<(int r, int c)>();
            var used = new HashSet<(int r, int c)>();

            for (int i = 1; i <= shots; i++)
            {
                while (true)
                {
                    var (r, c) = ReadCoord($"Shot {i}/{shots} (r,c): ");
                    if (!enemy.InBounds(r, c)) { Console.WriteLine("OUT OF BOUNDS. Try again."); continue; }
                    if (used.Contains((r, c))) { Console.WriteLine("DUPLICATE in this volley. Try again."); continue; }
                    // permit re-firing at old enemy cell? Original forbids iirc; let’s forbid to keep it clean
                    if (enemy.Grid[r, c] == Cell.Miss || enemy.Grid[r, c] == Cell.Hit) { Console.WriteLine("YOU FIRED THERE BEFORE. Try again."); continue; }
                    used.Add((r, c));
                    list.Add((r, c));
                    break;
                }
            }
            return list;
        }

        List<(int r, int c)> EnemyVolley(int shots)
        {
            var volley = new List<(int r, int c)>();
            int tries = 0;
            while (volley.Count < shots && tries++ < 5000)
            {
                int r = rng.Next(1, Board.N + 1);
                int c = rng.Next(1, Board.N + 1);
                if (enemyShotsTried.Contains((r, c))) continue;
                enemyShotsTried.Add((r, c));
                volley.Add((r, c));
            }
            return volley;
        }

        // -------- Utilities --------

        static (int r, int c) ReadCoord(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = (Console.ReadLine() ?? "").Trim();
                // Accept forms: "r,c" or "r c"
                var parts = s.Split(new[] { ',', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2) { Console.WriteLine("Enter as row,col (e.g., 4,7)."); continue; }
                if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int r) ||
                    !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int c))
                { Console.WriteLine("Row/col must be integers 1..10."); continue; }
                if (r < 1 || r > Board.N || c < 1 || c > Board.N) { Console.WriteLine("Row/col must be 1..10."); continue; }
                return (r, c);
            }
        }

        static bool ReadYesNo()
        {
            while (true)
            {
                var s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
                if (s.StartsWith("Y")) return true;
                if (s.StartsWith("N")) return false;
                Console.Write("Please answer YES or NO: ");
            }
        }

        static void PrintFleetSummary(Board b)
        {
            foreach (var s in b.Fleet)
            {
                string cells = string.Join(" ", s.Cells.Select(p => $"({p.r},{p.c})"));
                Console.WriteLine($"  {s.Type.Name,-12} len={s.Type.Length} at {cells}");
            }
        }
    }
}
