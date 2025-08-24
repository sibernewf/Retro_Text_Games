using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace SubmarineGame
{
    class Vec
    {
        public double X, Y;
        public Vec(double x, double y) { X = x; Y = y; }
        public static Vec operator +(Vec a, Vec b) => new(a.X + b.X, a.Y + b.Y);
        public static Vec operator -(Vec a, Vec b) => new(a.X - b.X, a.Y - b.Y);
        public static Vec operator *(Vec a, double k) => new(a.X * k, a.Y * k);
        public double Len() => Math.Sqrt(X * X + Y * Y);
        public Vec Unit() { var L = Len(); return L == 0 ? new(1, 0) : new(X / L, Y / L); }
        public override string ToString() => $"({X:0.0},{Y:0.0})";
        public static Vec FromPolarDeg(double r, double deg)
        {
            double rad = deg * Math.PI / 180.0;
            return new Vec(r * Math.Cos(rad), r * Math.Sin(rad));
        }
    }

    class Scenario
    {
        // Units:
        //   Distance: yards
        //   Time: minutes
        //   Speed: knots (nautical miles per hour) → yards/min via KNOT = 2025/60 ≈ 33.75 yd/min
        private const double YardsPerMinPerKnot = 2025.0 / 60.0;

        readonly Random rng = new();
        public Vec SubPos = new(0, 0);                 // Sub stays at origin
        public Vec BoatPos = new(0, 0);                // ✅ initialized to satisfy nullable analysis
        public Vec BoatVel = new(0, 0);                // ✅ initialized to satisfy nullable analysis
        public double TorpedoSpeedKts = 50;            // classic fast fish
        public double HitRadiusYds = 100;              // proximity fuse
        public int Torpedoes = 4;                      // shots before they get you
        public List<string> Log = new();

        public void NewEncounter()
        {
            // Random initial range ~ 2500–5000 yds, bearing 20°..340° (not straight through the sub)
            double range = rng.Next(2500, 5001);
            double brg = rng.Next(20, 341);
            BoatPos = Vec.FromPolarDeg(range, brg);

            // Gunboat speed 12–32 kts, heading 0–359°
            double spdKts = rng.Next(12, 33);
            double hdg = rng.Next(0, 360);
            BoatVel = Vec.FromPolarDeg(spdKts * YardsPerMinPerKnot, hdg);

            Log.Add($"NEW CONTACT: range {range:0} yds, bearing {brg}°, speed {spdKts} kts, course {hdg}°");
        }

        public (bool hit, double missDist) SimulateShot(double fireAngleDeg, double maxMinutes = 12.0)
        {
            // Torpedo starts now at sub position, straight course at fixed speed
            Vec torpVel = Vec.FromPolarDeg(1, fireAngleDeg).Unit() * (TorpedoSpeedKts * YardsPerMinPerKnot);
            Vec pT = SubPos; Vec pB = BoatPos;
            double dt = 0.05; // 3 seconds

            double best = double.MaxValue;
            for (double t = 0; t <= maxMinutes; t += dt)
            {
                double d = (pT - pB).Len();
                if (d < best) best = d;
                if (d <= HitRadiusYds) return (true, d);
                // advance
                pT += torpVel * dt;
                pB += BoatVel * dt;
            }
            return (false, best);
        }

        // Optional helper: a rough “advice band” of angles that would pass within 300 yds if fired now
        public (int lo, int hi) SuggestedAngleBand()
        {
            int lo = -1, hi = -1;
            for (int ang = 0; ang < 360; ang++)
            {
                var r = SimulateShot(ang, 8);
                if (!r.hit && r.missDist > 300) continue;
                if (lo < 0) lo = ang;
                hi = ang;
            }
            if (lo < 0) { lo = 0; hi = 359; }
            // compress to a 0..180 mirror band for friendlier hint
            int mid = (lo + hi) / 2;
            int w = Math.Min(90, (hi - lo) / 2 + 5);
            return (Normalize(mid - w), Normalize(mid + w));

            static int Normalize(int d) => (d % 360 + 360) % 360;
        }
    }

    class Game
    {
        readonly Scenario S = new();
        readonly List<string> SessionLog = new();

        public void Run()
        {
            Intro();
            while (true)
            {
                S.NewEncounter();
                PlayEncounter();
                if (!AskYesNo("\nWOULD YOU LIKE TO TRY AGAIN? (Y/N) ")) break;
            }

            // save session log
            var path = "submarine_log.txt";
            File.WriteAllLines(path, SessionLog.Concat(S.Log));
            Console.WriteLine($"\nLog saved to: {Path.GetFullPath(path)}");
        }

        void PlayEncounter()
        {
            var (lo, hi) = S.SuggestedAngleBand();
            Console.WriteLine($"\nINTEL: Torpedo speed {S.TorpedoSpeedKts} kts. Gunboat speed unknown.");
            Console.WriteLine($"RANGE TO TARGET ≈ {(S.BoatPos - S.SubPos).Len():0} yards.");
            Console.WriteLine($"EARLY GUESS: try angles between {lo}° and {hi}°.");

            for (int shot = 1; shot <= S.Torpedoes; shot++)
            {
                Console.Write($"\nSHOT #{shot} — ENTER FIRING ANGLE (0–359°, Q quits): ");
                string s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
                if (s == "Q") Quit();
                if (!int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out int angle) ||
                    angle < 0 || angle > 359)
                {
                    Console.WriteLine("Invalid angle. Try an integer 0–359.");
                    shot--; continue;
                }

                var (hit, miss) = S.SimulateShot(angle);
                if (hit)
                {
                    Console.WriteLine("\n********** YOU MADE A QUESTIONABLE STRIKE! **********");
                    Console.WriteLine("DIRECT HIT! THE GUNBOAT IS SUNK. GOOD BYE, CRUEL WORLD… (GLOOB…GLOOB…)");
                    SessionLog.Add($"HIT at angle {angle}°, shot {shot}");
                    return;
                }
                else
                {
                    Console.WriteLine($"SPLASH — MISS. CLOSEST APPROACH ≈ {miss:0} yards.");
                    SessionLog.Add($"MISS at {angle}°, CA {miss:0} yds");
                }
            }

            // If you used all torpedoes, you die
            Console.WriteLine("\nTHE GUNBOAT HAS ZEROED YOUR POSITION AND RETURNS FIRE…");
            Console.WriteLine("I AM SINK’N………GOOD BYE CRUEL WORLD… (GLOUB..GLOUB..)");
        }

        void Intro()
        {
            Console.WriteLine("THIS IS THE GAME OF WAR BETWEEN A SUBMARINE AND A NAVAL GUN BOAT.");
            Console.WriteLine("You are the captain of the SUB; the other guy is fast and angry.");
            Console.WriteLine("Pick a FIRING ANGLE in degrees (0–359). Torpedo runs straight at fixed speed.");
            Console.WriteLine("If your track passes within ~100 yards of the gunboat, it's a HIT.");
            Console.WriteLine("You have 4 torpedoes before they depth-charge you. Good luck!\n");
            Console.WriteLine("Type Q at any prompt to quit.");
        }

        static bool AskYesNo(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
                if (s == "Q") Quit();
                if (s.StartsWith("Y")) return true;
                if (s.StartsWith("N")) return false;
            }
        }

        static void Quit()
        {
            Console.WriteLine("Quitting…");
            Environment.Exit(0);
        }
    }

    class Program
    {
        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            new Game().Run();
        }
    }
}
