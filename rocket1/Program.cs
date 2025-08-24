using System;
using System.Globalization;

namespace Rocket1
{
    internal static class Program
    {
        // Physics knobs (chosen to line up with the classic ROCKT1 table)
        const double G = 13.0;          // lunar gravity used by the original variant (ft/s^2)
        const int MaxThrust = 30;       // maximum units you can burn per second
        const int StartAltitude = 500;  // feet
        const int StartSpeed = 50;      // ft/s downward
        const int StartFuel = 120;      // units
        const double PerfectThresh = 2.0; // ≤ 2 ft/s => perfect
        const double GoodThresh = 5.0;    // ≤ 5 ft/s => good

        static void Main()
        {
            Console.WriteLine("ROCKET1 — Lunar Landing (1-second bursts)\n");

            if (AskYesNo("Do you want instructions (YES or NO)? "))
            {
                ShowInstructions();
            }

            do
            {
                RunMission();
            } while (AskYesNo("\nAnother mission? "));
        }

        private static void RunMission()
        {
            double altitude = StartAltitude; // ft
            double speed = StartSpeed;       // ft/s downward
            int fuel = StartFuel;            // units
            int sec = 0;

            Console.WriteLine();
            Console.WriteLine("BEGINNING LANDING PROCEDURE.");
            Console.WriteLine(" GOOD L U C K !!\n");
            Console.WriteLine(" SEC   FEET   SPEED   FUEL   PLOT OF DISTANCE");

            // Print first status line at t = 0
            PrintRow(sec, altitude, speed, fuel);

            while (true)
            {
                // If we have already touched down (can happen if starting altitude <= 0)
                if (altitude <= 0)
                {
                    ReportTouchdown(sec, 0.0, speed, fuel);
                    return;
                }

                // Ask for burn for the coming second (if any fuel remains)
                int burn = 0;
                if (fuel > 0)
                {
                    burn = ReadIntClamped($" ? ", 0, Math.Min(MaxThrust, fuel));
                }
                else
                {
                    Console.WriteLine(" OUT OF FUEL.");
                }

                // Physics for the next 1-second interval
                // a = g - burn (positive increases downward speed)
                double a = G - burn;

                // Will we hit the surface within this 1-second interval?
                // Solve h(t) = altitude - speed*t - 0.5*a*t^2 = 0   for t in [0,1]
                // If a == 0 => t = altitude / speed (if speed > 0)
                double tHit = TimeToContactInThisSecond(altitude, speed, a);

                fuel -= burn;

                if (tHit >= 0.0 && tHit <= 1.0)
                {
                    // We touch down within this second — compute precise impact speed
                    double impactV = speed + a * tHit;
                    ReportTouchdown(sec, tHit, impactV, fuel);
                    return;
                }

                // Otherwise complete the full second
                altitude = altitude - speed - 0.5 * a; // s_new = s - v*1 - 0.5*a*1^2
                speed = speed + a;
                sec++;

                PrintRow(sec, Math.Max(0, altitude), speed, fuel);

                // Safety: if altitude plunged below zero because of numeric edge, finish next loop
                if (altitude <= 0)
                {
                    // Compute exact touchdown within *last* step retrospectively
                    // Back-solve using previous state:
                    // Step back one second (approximate):
                    // We'll reconstruct previous state to get accurate touch time.
                    // Previous state:
                    double prevAlt = altitude + speed - a * 0.5; // undo: s' = s - v - 0.5a -> s = s' + v + 0.5a
                    double prevV   = speed - a;                  // undo v' = v + a -> v = v' - a

                    double t2 = TimeOfImpact(prevAlt, prevV, a);
                    double vImpact = prevV + a * t2;
                    ReportTouchdown(sec - 1, t2, vImpact, fuel);
                    return;
                }
            }
        }

        /// <summary>
        /// If contact occurs within the *next* second from (altitude, speed, a), return t in [0,1]; else -1.
        /// </summary>
        private static double TimeToContactInThisSecond(double h, double v, double a)
        {
            double t = TimeOfImpact(h, v, a);
            return (t >= 0.0 && t <= 1.0) ? t : -1.0;
        }

        /// <summary>
        /// Solve h(t) = h - v t - 0.5 a t^2 = 0 for t >= 0.
        /// Returns -1 if no valid positive root.
        /// </summary>
        private static double TimeOfImpact(double h, double v, double a)
        {
            // Handle a ~ 0: linear descent
            const double EPS = 1e-9;
            if (Math.Abs(a) < EPS)
            {
                if (v <= 0) return -1; // not descending
                return h / v;
            }

            // 0.5 a t^2 + v t - h = 0  -> use quadratic formula
            double A = 0.5 * a;
            double B = v;
            double C = -h;

            double disc = B * B - 4 * A * C;
            if (disc < 0) return -1;

            double sqrt = Math.Sqrt(disc);
            // We need the positive root
            double t1 = (-B + sqrt) / (2 * A);
            double t2 = (-B - sqrt) / (2 * A);

            double t = double.MaxValue;
            if (t1 >= 0) t = Math.Min(t, t1);
            if (t2 >= 0) t = Math.Min(t, t2);
            return (t == double.MaxValue) ? -1 : t;
        }

        private static void ReportTouchdown(int wholeSeconds, double frac, double impactV, int fuel)
        {
            double tTotal = wholeSeconds + frac;
            Console.WriteLine();
            Console.WriteLine($" TOUCHDOWN AT {tTotal:F4} SECONDS.");
            Console.WriteLine($" LANDING VELOCITY = {impactV:F4} FT/SEC");
            Console.WriteLine($" {fuel} UNITS OF FUEL REMAINING.");

            if (impactV <= PerfectThresh)
            {
                Console.WriteLine(" CONGRATULATIONS! A PERFECT LANDING!");
                Console.WriteLine(" YOUR LICENSE WILL BE RENEWED... LATER.");
            }
            else if (impactV <= GoodThresh)
            {
                Console.WriteLine(" GOOD LANDING (COULD BE BETTER).");
            }
            else
            {
                Console.WriteLine(" *** SORRY, BUT YOU BLEW IT! ***");
                Console.WriteLine(" APPROPRIATE CONDOLENCES WILL BE SENT TO YOUR NEXT OF KIN.");
            }
        }

        private static void PrintRow(int sec, double feet, double speed, int fuel)
        {
            // Simple distance plotter (one tick per ~25 ft fallen from start)
            // For a nostalgic feel you can add dots proportional to elapsed time/altitude
            Console.WriteLine($"{sec,3} {Math.Max(0, (int)Math.Round(feet)),6} {speed,7:F0} {fuel,6}  {Plot(feet)}");
        }

        private static string Plot(double feet)
        {
            int ticks = Math.Max(0, (int)((StartAltitude - feet) / 25.0));
            return new string('.', Math.Min(ticks, 40));
        }

        private static bool AskYesNo(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = Console.ReadLine()?.Trim().ToUpperInvariant();
                if (string.IsNullOrEmpty(s)) continue;
                if (s.StartsWith("Y")) return true;
                if (s.StartsWith("N")) return false;
                Console.WriteLine("Please answer YES or NO.");
            }
        }

        private static int ReadIntClamped(string prompt, int min, int max)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = Console.ReadLine();
                if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v))
                {
                    if (v < min) v = min;
                    if (v > max) v = max;
                    return v;
                }
                Console.WriteLine("Enter a whole number, please.");
            }
        }

        private static void ShowInstructions()
        {
            Console.WriteLine(@"
You are landing on the Moon and have taken over manual control.
You begin 500 feet above the surface, descending at 50 ft/sec, with 120 fuel units.

Every SECOND you'll enter how many fuel units to burn (0–30).
Each unit reduces your descent by 1 ft/sec for that second.
Gravity acts downward at 13 ft/sec^2 every second.

After each second, you'll see: elapsed seconds, altitude (ft), speed (ft/s), and fuel left.
When you touch down, the simulator computes the exact time within the last second and your impact speed.

Landing ratings:
  ≤ 2.0 ft/s  : PERFECT
  ≤ 5.0 ft/s  : GOOD
  > 5.0 ft/s  : CRASH

Be careful not to slow down too soon and run out of fuel high above the surface.
Good luck, pilot!
");
        }
    }
}
