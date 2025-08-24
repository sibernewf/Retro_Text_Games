using System;
using System.Globalization;
using System.IO;

namespace OrbitGame
{
    internal static class Program
    {
        private static readonly Random Rng = new Random();

        // World settings (miles)
        private const int MinOrbit = 10_000;     // ship radius from origin
        private const int MaxOrbit = 30_000;
        private const int DestroyRadius = 5_000; // within this range = hit
        private const int MinBombR = 1_000;      // input limits for bomb radius
        private const int MaxBombR = 30_000;

        // Time / shots
        private const int MinPeriodHours = 12;   // ship period (12–36 h)
        private const int MaxPeriodHours = 36;
        private const int Shots = 7;             // seven bombs (hours)

        // Radar drawing (square grid with center)
        private const int Grid = 41;             // odd number; 41 x 41
        private static string LogPath = string.Empty;

        static void Main()
        {
            Console.Title = "ORBIT — Destroy an Orbiting Enemy Spaceship";

            // Setup logging
            LogPath = Path.Combine(AppContext.BaseDirectory, $"orbit-log-{DateTime.Now:yyyyMMdd-HHmmss}.txt");
            using (var w = File.CreateText(LogPath))
            {
                w.WriteLine($"=== ORBIT LOG === {DateTime.Now}");
            }

            Intro();

            // Hidden ship parameters
            double R = Rng.Next(MinOrbit, MaxOrbit + 1);           // miles
            double period = Rng.Next(MinPeriodHours, MaxPeriodHours + 1); // hours
            double omega = 360.0 / period;                          // deg/hour
            double theta0 = Rng.NextDouble() * 360.0;               // initial angle (deg)

            AppendLog($"Ship orbit radius R = {R:F0} miles");
            AppendLog($"Ship period = {period:F0} h (ω = {omega:F3}°/h)");
            AppendLog($"Ship initial angle θ0 = {theta0:F3}°");

            Console.WriteLine("GOOD LUCK. THE FEDERATION IS COUNTING ON YOU.\n");

            bool destroyed = false;
            for (int hour = 1; hour <= Shots; hour++)
            {
                Console.WriteLine($"HOUR {hour}/{Shots} — AT WHAT ANGLE DO YOU WISH TO SEND YOUR PROTON BOMB?");
                double bombAngle = ReadDouble("ANGLE (0..360°): ", 0, 360);
                double bombR = ReadDouble($"RADIUS FROM ORIGIN (miles {MinBombR}..{MaxBombR}): ", MinBombR, MaxBombR);

                // True ship angle this hour
                double shipAngle = Normalize(theta0 + omega * (hour - 1));

                // Distance by law of cosines:
                // D = sqrt( R^2 + D1^2 - 2 R D1 cos(A - A1) )
                double d = DistanceByCosines(R, bombR, bombAngle, shipAngle);

                Console.WriteLine($"\nYOUR PROTON BOMB EXPLODED {bombAngle:F3}° @ {bombR:F0} MILES FROM THE ORIGIN.");
                Console.WriteLine($"DISTANCE FROM ENEMY SHIP = {d:F0} MILES.\n");

                // Draw radar with your bomb marked (ship remains invisible)
                DrawRadar(bombAngle, bombR);

                AppendLog($"Hour {hour}: bomb A={bombAngle:F3}°, r={bombR:F0} mi | ship A={shipAngle:F3}°, r={R:F0} mi | distance={d:F1} mi");

                if (d <= DestroyRadius)
                {
                    destroyed = true;
                    Console.WriteLine("*** DIRECT HIT! THE ENEMY SHIP IS DESTROYED. ***");
                    AppendLog("Destroyed: YES");
                    break;
                }
            }

            if (!destroyed)
            {
                Console.WriteLine("YOU HAVE SUCCESSFULLY COMPLETED YOUR MISSION?");
                Console.WriteLine("...UNFORTUNATELY NO — THE ENEMY ESCAPED THIS TIME.");
                AppendLog("Destroyed: NO");
            }

            Console.WriteLine($"\nLog written to: {LogPath}");
        }

        // ---------- Physics / math ----------
        private static double DistanceByCosines(double r1, double r2, double a1Deg, double a2Deg)
        {
            double delta = DegreesToRadians(Normalize(a1Deg - a2Deg));
            return Math.Sqrt(r1 * r1 + r2 * r2 - 2.0 * r1 * r2 * Math.Cos(delta));
        }

        private static double Normalize(double deg)
        {
            deg %= 360.0;
            if (deg < 0) deg += 360.0;
            return deg;
        }

        private static double DegreesToRadians(double deg) => deg * Math.PI / 180.0;

        // ---------- UI ----------
        private static void Intro()
        {
            Console.WriteLine("SOMEWHERE ABOVE YOUR PLANET IS A CLOAKED ENEMY SHIP.\n");
            Console.WriteLine("THIS SHIP IS IN CONSTANT POLAR ORBIT (COUNTERCLOCKWISE).");
            Console.WriteLine($"ITS DISTANCE FROM THE ORIGIN IS BETWEEN {MinOrbit:N0} AND {MaxOrbit:N0} MILES.");
            Console.WriteLine($"PERIOD IS 1 REVOLUTION EVERY {MinPeriodHours}..{MaxPeriodHours} HOURS.\n");
            Console.WriteLine($"YOU HAVE {Shots} HOURS. EACH HOUR, ENTER A BOMB ANGLE (0..360°) AND A RADIUS IN MILES.");
            Console.WriteLine($"AN EXPLOSION WITHIN {DestroyRadius:N0} MILES OF THE ENEMY SHIP WILL DESTROY IT.\n");
            Console.WriteLine("BELOW IS A RADAR-LIKE DIAGRAM (0°, 90°, 180°, 270° AXES).");
            Console.WriteLine("THE ENEMY IS INVISIBLE; YOUR BOMB LOCATION IS MARKED WITH '*'.\n");
        }

        // ASCII radar (square grid) with your bomb marked
        private static void DrawRadar(double bombAngleDeg, double bombRadius)
        {
            char[,] g = new char[Grid, Grid];
            for (int r = 0; r < Grid; r++)
                for (int c = 0; c < Grid; c++)
                    g[r, c] = ' ';

            int cx = Grid / 2, cy = Grid / 2;

            // Crosshairs
            for (int i = 0; i < Grid; i++) { g[cy, i] = '-'; g[i, cx] = '|'; }
            g[cy, cx] = '+';

            // Concentric rings (every 5,000 mi)
            int maxR = MaxOrbit; // scale reference
            double scale = (Grid - 1) / 2.0 / maxR; // miles -> grid cells
            for (int ring = 5_000; ring <= MaxOrbit; ring += 5_000)
            {
                double rr = ring * scale;
                PlotCircle(g, cx, cy, rr, '.');
            }

            // Bomb mark
            int bx, by;
            PolarToGrid(bombAngleDeg, bombRadius, scale, cx, cy, out bx, out by);
            if (InGrid(bx, by)) g[by, bx] = '*';

            // Labels
            WriteString(g, 0, cy - 1, "270");
            WriteString(g, Grid - 3, cy - 1, "090");
            WriteString(g, cx - 1, 0, " 90");
            WriteString(g, cx - 1, Grid - 2, "180");

            // Print
            for (int r = 0; r < Grid; r++)
            {
                for (int c = 0; c < Grid; c++)
                    Console.Write(g[r, c]);
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        private static void PlotCircle(char[,] g, int cx, int cy, double r, char ch)
        {
            // simple Bresenham-ish sampling
            for (double t = 0; t < 2 * Math.PI; t += 0.02)
            {
                int x = cx + (int)Math.Round(r * Math.Cos(t));
                int y = cy - (int)Math.Round(r * Math.Sin(t));
                if (InGrid(x, y) && g[y, x] == ' ')
                    g[y, x] = ch;
            }
        }

        private static void PolarToGrid(double angleDeg, double radius,
                                        double scale, int cx, int cy,
                                        out int x, out int y)
        {
            double a = DegreesToRadians(angleDeg);
            double rr = radius * scale;
            x = cx + (int)Math.Round(rr * Math.Cos(a));
            y = cy - (int)Math.Round(rr * Math.Sin(a));
        }

        private static void WriteString(char[,] g, int x, int y, string s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                int xx = x + i;
                if (InGrid(xx, y)) g[y, xx] = s[i];
            }
        }

        private static bool InGrid(int x, int y) => x >= 0 && x < Grid && y >= 0 && y < Grid;

        // ---------- Input / Log ----------
        private static double ReadDouble(string prompt, double min, double max)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = (Console.ReadLine() ?? "").Trim();
                if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out double v)
                    && v >= min && v <= max)
                    return v;

                Console.WriteLine($"ENTER A NUMBER BETWEEN {min} AND {max}.");
            }
        }

        private static void AppendLog(string line)
        {
            using var w = File.AppendText(LogPath);
            w.WriteLine(line);
        }
    }
}
