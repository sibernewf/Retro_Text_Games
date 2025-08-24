using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Spacwr
{
    class Program
    {
        static void Main()
        {
            Console.Title = "SPACWR — Spacewar (Star Trek)";
            new Game().Run();
        }
    }

    sealed class Game
    {
        const int NQ = 8;                  // 8x8 quadrants
        const int NS = 8;                  // 8x8 sectors
        const int START_KLINGONS = 24;     // total in galaxy
        const int START_BASES = 3;         // total starbases
        const int START_ENERGY = 3000;
        const int START_TORPS = 10;
        const int STAR_LIMIT = 30;         // stardates

        readonly Random rng = new Random();

        // Galaxy cell: number of K, B, * in each quadrant
        struct Quad { public int K, B, S; public bool Visited; }
        Quad[,] galaxy = new Quad[NQ, NQ];

        // Current quadrant & sector
        int qx, qy;          // 0..7
        int sx, sy;          // 0..7

        // Sector map: '.', '*', 'K', 'B', 'E'
        char[,] sector = new char[NS, NS];

        // Player state
        int energy = START_ENERGY;
        int shields = 0;
        int torps = START_TORPS;
        double stardate = 0;

        bool docked = false;

        public void Run()
        {
            Intro();
            GenerateGalaxy();
            // start somewhere random, then populate that quadrant
            qx = rng.Next(NQ); qy = rng.Next(NQ);
            EnterQuadrant();
            Loop();
        }

        void Intro()
        {
            Console.WriteLine("SPACEWAR — You are Captain of the Enterprise.");
            Console.WriteLine($"Destroy all {START_KLINGONS} Klingons in {STAR_LIMIT} stardates.\n");
            Console.WriteLine("Commands:");
            Console.WriteLine("  1 = Short Range Scan     2 = Long Range Scan");
            Console.WriteLine("  3 = Warp Move            4 = Fire Phasers");
            Console.WriteLine("  5 = Fire Photon Torpedo  6 = Shield/Repair/Dock");
            Console.WriteLine("  7 = Status               0 = Help / Quit\n");
        }

        // ---------- Galaxy generation ----------

        void GenerateGalaxy()
        {
            int remainingK = START_KLINGONS;
            int remainingB = START_BASES;

            // sprinkle stars, K, bases
            for (int x = 0; x < NQ; x++)
            for (int y = 0; y < NQ; y++)
            {
                galaxy[x, y].S = rng.Next(1, 10) + rng.Next(0, 6); // 1..14 stars
                galaxy[x, y].K = 0;
                galaxy[x, y].B = 0;
            }

            while (remainingK > 0)
            {
                int x = rng.Next(NQ), y = rng.Next(NQ);
                if (galaxy[x, y].K < 3) { galaxy[x, y].K++; remainingK--; }
            }
            while (remainingB > 0)
            {
                int x = rng.Next(NQ), y = rng.Next(NQ);
                if (galaxy[x, y].B == 0) { galaxy[x, y].B = 1; remainingB--; }
            }
        }

        // ---------- Quadrant entry / build sector map ----------

        void EnterQuadrant()
        {
            // clear sector
            for (int x = 0; x < NS; x++)
                for (int y = 0; y < NS; y++)
                    sector[x, y] = '.';

            // place stars
            for (int i = 0; i < galaxy[qx, qy].S; i++) PlaceRandom('*');

            // place base(s)
            for (int i = 0; i < galaxy[qx, qy].B; i++) PlaceRandom('B');

            // place klingons
            for (int i = 0; i < galaxy[qx, qy].K; i++) PlaceRandom('K');

            // place Enterprise
            PlaceRandom('E', out sx, out sy);

            docked = IsAdjacentToBase();
            galaxy[qx, qy].Visited = true;
        }

        void PlaceRandom(char ch) => PlaceRandom(ch, out _, out _);

        void PlaceRandom(char ch, out int x, out int y)
        {
            int tries = 0;
            while (true)
            {
                x = rng.Next(NS); y = rng.Next(NS);
                if (sector[x, y] == '.') { sector[x, y] = ch; return; }
                if (++tries > 500) throw new Exception("Sector fill overflow");
            }
        }

        // ---------- Game loop ----------

        void Loop()
        {
            while (true)
            {
                if (galaxy.Cast<Quad>().Sum(q => q.K) == 0)
                {
                    Console.WriteLine("\n*** MISSION ACCOMPLISHED ***");
                    Console.WriteLine($"All Klingons destroyed in {stardate:0.0} stardates.");
                    return;
                }
                if (stardate >= STAR_LIMIT)
                {
                    Console.WriteLine("\n*** MISSION FAILED *** Time has run out.");
                    return;
                }
                if (energy <= 0)
                {
                    Console.WriteLine("\n*** The Enterprise has no energy. You drift forever. ***");
                    return;
                }

                Console.Write($"\nStardate {stardate:0.0}  Quad {qy + 1},{qx + 1}  Sec {sy + 1},{sx + 1}  E:{energy}  S:{shields}  T:{torps}  K:{galaxy[qx,qy].K}\n");
                int cmd = AskInt("Command (0=Help): ", 0, 7);

                switch (cmd)
                {
                    case 0: Help(); break;
                    case 1: ShortScan(); break;
                    case 2: LongScan(); break;
                    case 3: Warp(); break;
                    case 4: FirePhasers(); break;
                    case 5: FireTorpedo(); break;
                    case 6: ShieldsDock(); break;
                    case 7: Status(); break;
                }

                if (KlingonsInQuadrant() > 0)
                    KlingonsFire();
            }
        }

        void Help()
        {
            Console.WriteLine("\nCommands:");
            Console.WriteLine("1 Short Range Scan (8×8 sector view)");
            Console.WriteLine("2 Long Range Scan (3×3 quadrants, code = K-B-S)");
            Console.WriteLine("3 Warp Move (course 1..9 keypad, warp 0.1..8)");
            Console.WriteLine("4 Fire Phasers (distributes energy at Klingons in this quadrant)");
            Console.WriteLine("5 Fire Photon Torpedo (flies in a straight line)");
            Console.WriteLine("6 Shield/Repair/Dock (transfer energy; dock if next to a base)");
            Console.WriteLine("7 Status (remaining Klingons/bases, stardate limit)");
        }

        // ---------- Scans ----------

        void ShortScan()
        {
            Console.WriteLine();
            for (int y = NS - 1; y >= 0; y--)
            {
                for (int x = 0; x < NS; x++)
                    Console.Write(sector[x, y]);
                Console.WriteLine();
            }
        }

        void LongScan()
        {
            Console.WriteLine("\nLONG RANGE SCAN (K-B-S):");
            for (int dy = 1; dy >= -1; dy--)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    int gx = qx + dx, gy = qy + dy;
                    if (gx < 0 || gx >= NQ || gy < 0 || gy >= NQ) { Console.Write(" *** "); continue; }
                    var q = galaxy[gx, gy];
                    Console.Write($" {q.K}{q.B}{Math.Min(q.S,9)} ");
                }
                Console.WriteLine();
            }
        }

        // ---------- Movement ----------

        void Warp()
        {
            double course = AskDouble("Course (1..9, 9=1): ", 1, 9.0001);
            double warp = AskDouble("Warp factor (0.1..8): ", 0.1, 8.0);
            // delta per step based on keypad course
            (double dx, double dy) = CourseToDelta(course);
            int steps = (int)Math.Round(warp * 8); // sectors moved
            if (steps <= 0) return;

            energy -= (int)Math.Ceiling(warp * 10);
            stardate += Math.Max(0.1, warp / 2);

            sector[sx, sy] = '.'; // leave current

            for (int i = 0; i < steps; i++)
            {
                int nsx = sx + (dx > 0 ? 1 : dx < 0 ? -1 : 0);
                int nsy = sy + (dy > 0 ? 1 : dy < 0 ? -1 : 0);
                double fracx = Math.Abs((sx + dx) - nsx);
                double fracy = Math.Abs((sy + dy) - nsy);
                // advance one sector at a time (simple)
                sx = nsx; sy = nsy;

                // crossed quadrant edge?
                if (sx < 0) { sx = NS - 1; qx--; EnterOrAbortMovement(); if (aborted) return; }
                if (sx >= NS){ sx = 0;      qx++; EnterOrAbortMovement(); if (aborted) return; }
                if (sy < 0) { sy = NS - 1; qy--; EnterOrAbortMovement(); if (aborted) return; }
                if (sy >= NS){ sy = 0;      qy++; EnterOrAbortMovement(); if (aborted) return; }

                // star collision stops movement just before obstacle
                if (sector[sx, sy] == '*')
                {
                    sx -= Math.Sign(dx); sy -= Math.Sign(dy);
                    Console.WriteLine("NAV: Movement blocked by star.");
                    break;
                }
                if (sector[sx, sy] == 'K' || sector[sx, sy] == 'B')
                {
                    sx -= Math.Sign(dx); sy -= Math.Sign(dy);
                    Console.WriteLine("NAV: Movement halted (object ahead).");
                    break;
                }
            }

            sector[sx, sy] = 'E';
            docked = IsAdjacentToBase();
        }

        bool aborted = false;
        void EnterOrAbortMovement()
        {
            if (qx < 0 || qx >= NQ || qy < 0 || qy >= NQ)
            {
                Console.WriteLine("NAV: You cannot leave the galaxy.");
                aborted = true;
                // clamp to edge
                qx = Math.Clamp(qx, 0, NQ - 1);
                qy = Math.Clamp(qy, 0, NQ - 1);
            }
            else
            {
                EnterQuadrant();
                aborted = false;
            }
        }

        (double dx, double dy) CourseToDelta(double c)
        {
            // keypad:
            //  7 8 9
            //  4 5 6
            //  1 2 3
            int ic = (int)Math.Round(c);
            return ic switch
            {
                1 => (-1, -1),
                2 => (0, -1),
                3 => (1, -1),
                4 => (-1, 0),
                5 => (0, 0),
                6 => (1, 0),
                7 => (-1, 1),
                8 => (0, 1),
                9 => (1, 1),
                _ => (0, 0)
            };
        }

        // ---------- Combat ----------

        int KlingonsInQuadrant()
        {
            int n = 0;
            for (int x = 0; x < NS; x++)
                for (int y = 0; y < NS; y++)
                    if (sector[x, y] == 'K') n++;
            return n;
        }

        IEnumerable<(int x,int y,double dist)> EnumerateKlingons()
        {
            for (int x = 0; x < NS; x++)
                for (int y = 0; y < NS; y++)
                    if (sector[x, y] == 'K')
                    {
                        double d = Math.Sqrt((x - sx) * (x - sx) + (y - sy) * (y - sy));
                        yield return (x, y, Math.Max(0.5, d));
                    }
        }

        void FirePhasers()
        {
            if (KlingonsInQuadrant() == 0) { Console.WriteLine("Computer: No Klingons in this quadrant."); return; }
            if (energy <= 0) { Console.WriteLine("Computer: No energy."); return; }
            int qty = AskInt("Phaser energy to fire (1..energy)? ", 1, energy);
            energy -= qty;

            // spread damage inversely with distance
            int hits = 0;
            foreach (var (x, y, d) in EnumerateKlingons().OrderBy(t => t.dist).ToList())
            {
                int damage = (int)Math.Round(qty / (KlingonsInQuadrant() * d));
                if (damage <= 0) continue;
                if (damage >= 200) // threshold to kill
                {
                    sector[x, y] = '.';
                    galaxy[qx, qy].K--;
                    Console.WriteLine($"*** Klingon destroyed at {y + 1},{x + 1}!");
                    hits++;
                }
                else
                {
                    Console.WriteLine($"Hit at {y + 1},{x + 1} (damage {damage}).");
                    hits++;
                }
            }
            if (hits == 0) Console.WriteLine("No effect.");
        }

        void FireTorpedo()
        {
            if (torps <= 0) { Console.WriteLine("Computer: No torpedoes left."); return; }
            double course = AskDouble("Torpedo course (1..9): ", 1, 9.0001);
            (double dx, double dy) = CourseToDelta(course);
            if (dx == 0 && dy == 0) { Console.WriteLine("Invalid course."); return; }

            torps--;
            double tx = sx + 0.5, ty = sy + 0.5;

            while (true)
            {
                tx += dx * 0.25; ty += dy * 0.25;
                int ix = (int)Math.Round(tx), iy = (int)Math.Round(ty);

                if (ix < 0 || ix >= NS || iy < 0 || iy >= NS)
                { Console.WriteLine("Torpedo missed (left quadrant)."); break; }

                char ch = sector[ix, iy];
                if (ch == '*') { Console.WriteLine("Torpedo hit a star and fizzled."); break; }
                if (ch == 'B') { Console.WriteLine("Torpedo struck a starbase! (That was friendly...)"); sector[ix, iy] = '.'; galaxy[qx, qy].B = Math.Max(0, galaxy[qx, qy].B - 1); break; }
                if (ch == 'K')
                {
                    sector[ix, iy] = '.';
                    galaxy[qx, qy].K--;
                    Console.WriteLine($"*** Klingon destroyed at {iy + 1},{ix + 1}!");
                    break;
                }
            }
        }

        void KlingonsFire()
        {
            if (docked) { Console.WriteLine("(Docked at base; enemy fire blocked.)"); return; }

            int n = KlingonsInQuadrant();
            if (n == 0) return;

            int total = 0;
            foreach (var (_,_,d) in EnumerateKlingons())
            {
                int dmg = (int)Math.Round(100 / d) + rng.Next(0, 30);
                total += dmg;
            }

            if (shields > 0)
            {
                int absorbed = Math.Min(shields, total);
                shields -= absorbed;
                total -= absorbed;
                if (absorbed > 0) Console.WriteLine($"Shields absorb {absorbed}.");
            }

            if (total > 0)
            {
                energy -= total;
                Console.WriteLine($"Enterprise hit for {total} damage! (energy now {energy})");
            }
        }

        // ---------- Shields / Dock ----------

        void ShieldsDock()
        {
            Console.WriteLine(docked ? "You are docked at a starbase." : "No base adjacent.");
            Console.WriteLine("1) Transfer to shields  2) Transfer to energy  3) Repair/Resupply (docked)  0) Cancel");
            int k = AskInt("> ", 0, 3);
            if (k == 1)
            {
                int amt = AskInt("Energy -> Shields amount: ", 0, energy);
                energy -= amt; shields += amt;
            }
            else if (k == 2)
            {
                int amt = AskInt("Shields -> Energy amount: ", 0, shields);
                shields -= amt; energy += amt;
            }
            else if (k == 3)
            {
                if (!docked) { Console.WriteLine("You must be docked (adjacent to B)."); return; }
                Console.WriteLine("Resupplying...");
                energy = START_ENERGY; shields = 200; torps = START_TORPS;
                stardate += 0.2;
            }
            docked = IsAdjacentToBase();
        }

        bool IsAdjacentToBase()
        {
            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                {
                    int nx = sx + dx, ny = sy + dy;
                    if (nx >= 0 && nx < NS && ny >= 0 && ny < NS && sector[nx, ny] == 'B') return true;
                }
            return false;
        }

        // ---------- Status ----------

        void Status()
        {
            int kleft = galaxy.Cast<Quad>().Sum(q => q.K);
            int bleft = galaxy.Cast<Quad>().Sum(q => q.B);
            Console.WriteLine($"\nSTATUS REPORT");
            Console.WriteLine($"KLINGONS LEFT : {kleft}");
            Console.WriteLine($"STARBASES LEFT: {bleft}");
            Console.WriteLine($"STARDATE      : {stardate:0.0} / {STAR_LIMIT}");
            Console.WriteLine($"ENERGY {energy}, SHIELDS {shields}, TORPEDOES {torps}");
        }

        // ---------- Input helpers ----------

        int AskInt(string prompt, int min, int max)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = (Console.ReadLine() ?? "").Trim();
                if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v) && v >= min && v <= max) return v;
                Console.WriteLine($"Enter an integer {min}..{max}.");
            }
        }

        double AskDouble(string prompt, double min, double max)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = (Console.ReadLine() ?? "").Trim();
                if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out double v) && v >= min && v <= max) return v;
                Console.WriteLine($"Enter a number {min}..{max}.");
            }
        }
    }
}
