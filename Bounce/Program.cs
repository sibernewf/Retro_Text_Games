using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Bounce
{
    class Program
    {
        const double g = 32.0; // ft/s^2 (gravity)

        static void Main()
        {
            Console.WriteLine("BOUNCE — plot of a bouncing ball (horizontal ASCII plot)");
            Console.WriteLine("Press Q at any prompt to quit.\n");

            while (true)
            {
                double v0 = AskDouble("Initial velocity (ft/s)? ", min: 0);
                double e  = AskDouble("Coefficient (0..1)? ", min: 0, max: 0.9999);
                double dt = AskDouble("Time increment Δt (sec)? ", min: 0.001);
                Console.Write("Total seconds to plot (default 6): ");
                var s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
                if (s == "Q") return;
                double totalSeconds = 6.0;
                if (!string.IsNullOrWhiteSpace(s) && double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var ts) && ts > 0) totalSeconds = ts;

                Plot(v0, e, dt, totalSeconds);

                Console.Write("\nAgain (Y/N)? ");
                if (!AskYes()) break;
                Console.WriteLine();
            }
        }

        static void Plot(double v0, double e, double dt, double totalSeconds)
        {
            // Simulate
            var samples = Simulate(v0, e, dt, totalSeconds);

            // Scale to a manageable number of rows (top=peak, bottom=0)
            double maxH = Math.Max(1e-6, samples.Max(p => p.h));
            int rows = Math.Clamp((int)Math.Ceiling(Math.Min(20, Math.Max(8, maxH / 1.0))), 8, 24); // 8..24 rows
            int cols = samples.Count;

            // Build grid
            char[,] grid = new char[rows, cols];
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++) grid[r, c] = ' ';

            // Plot points
            for (int i = 0; i < cols; i++)
            {
                int r = RowFromHeight(samples[i].h, maxH, rows);
                if (r >= 0 && r < rows) grid[r, i] = 'o';
            }

            // Render with left scale (feet) and bottom seconds
            Console.WriteLine();
            for (int r = 0; r < rows; r++)
            {
                double feet = (rows - 1 - r) * maxH / (rows - 1);
                Console.Write($"{feet,5:0} | ");
                for (int c = 0; c < cols; c++) Console.Write(grid[r, c]);
                Console.WriteLine();
            }
            Console.Write("      + ");
            for (int c = 0; c < cols; c++) Console.Write('-');
            Console.WriteLine();

            // Seconds tick marks every ~1 sec
            Console.Write("Secs  : ");
            int stepPerSec = (int)Math.Round(1.0 / dt);
            if (stepPerSec < 1) stepPerSec = 1;
            for (int c = 0; c < cols; c++)
            {
                if (c % stepPerSec == 0) Console.Write('|'); else Console.Write(' ');
            }
            Console.WriteLine();

            Console.Write("        ");
            for (int c = 0; c < cols; c++)
            {
                if (c % stepPerSec == 0)
                {
                    int sec = (int)Math.Round(c * dt);
                    var label = sec.ToString(CultureInfo.InvariantCulture);
                    // print label starting at this column
                    Console.Write(label);
                    c += Math.Max(0, label.Length - 1);
                }
                else Console.Write(' ');
            }
            Console.WriteLine();
        }

        // Physics simulation: vertical throw with elastic bounces at y=0
        static List<(double t, double h)> Simulate(double v0, double e, double dt, double totalSeconds)
        {
            double t = 0;
            double y = 0;     // start at ground, initial upward velocity v0
            double v = v0;
            var list = new List<(double t, double h)>();
            int maxSteps = Math.Min(2000, (int)Math.Ceiling(totalSeconds / dt) + 1);

            for (int i = 0; i < maxSteps; i++)
            {
                list.Add((t, y));
                // step
                double vNext = v - g * dt;
                double yNext = y + v * dt - 0.5 * g * dt * dt;

                // Bounce handling if we cross the ground
                if (yNext < 0)
                {
                    // linearly find impact time within dt, reflect velocity with loss e, and continue remainder
                    double a = -0.5 * g;
                    double b = v;
                    double c = y; // y(t) = a*dt^2 + b*dt + c
                    // Solve a*τ^2 + b*τ + c = 0 for τ in (0,dt]
                    double disc = b * b - 4 * a * c;
                    double tau = dt;
                    if (disc >= 0)
                    {
                        double t1 = (-b + Math.Sqrt(disc)) / (2 * a);
                        double t2 = (-b - Math.Sqrt(disc)) / (2 * a);
                        // choose the root within (0,dt]
                        foreach (var cand in new[] { t1, t2 })
                            if (cand > 0 && cand <= dt) { tau = cand; break; }
                    }
                    // at impact
                    double vImpact = v - g * tau;
                    // reflected velocity
                    double vAfter = -e * vImpact;
                    double rem = dt - tau;
                    // advance remainder with new velocity from y=0
                    yNext = vAfter * rem - 0.5 * g * rem * rem;
                    vNext = vAfter - g * rem;
                    if (yNext < 0) { yNext = 0; vNext = 0; } // clamp
                }

                y = yNext;
                v = vNext;
                t += dt;
            }

            return list;
        }

        static int RowFromHeight(double h, double maxH, int rows)
        {
            if (maxH <= 0) return rows - 1;
            double frac = Math.Clamp(h / maxH, 0, 1);
            int r = (int)Math.Round((1 - frac) * (rows - 1));
            return r;
        }

        static double AskDouble(string prompt, double min = double.NegativeInfinity, double max = double.PositiveInfinity)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
                if (s == "Q") Environment.Exit(0);
                if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out double v) && v >= min && v <= max)
                    return v;
                Console.WriteLine($"Please enter a number between {min} and {max} (Q quits).");
            }
        }

        static bool AskYes()
        {
            while (true)
            {
                var s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
                if (s == "Q") return false;
                if (s.StartsWith("Y")) return true;
                if (s.StartsWith("N")) return false;
                Console.Write("Y or N (Q quits): ");
            }
        }
    }
}
