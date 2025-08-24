using System;
using System.Globalization;

namespace RocketLander
{
    internal static class Program
    {
        // ---- Physical constants / model ----
        // Work in feet and seconds. Positive velocity is downward.
        const double G = 5.31;                 // Moon gravity ~ ft/s^2
        const double MaxBurn = 200.0;          // lb/s input range
        const double IgnitionThreshold = 8.0;  // 1..7 has no effect
        const double Dt = 10.0;                // seconds per command
        const double ThrustAtMax = 20.0;       // ft/s^2 decel at burn=200
        // linear mapping: a_thrust = (burn/MaxBurn) * ThrustAtMax

        // ---- Scenario (classic defaults) ----
        const double StartAltMiles = 120.0; // 120 miles
        const double StartVelMPH = 3600.0;  // 3600 mph downward
        const double CapsuleWeight = 32500; // informational only
        const double StartFuel = 16500;     // lbs of fuel

        static void Main()
        {
            Console.Title = "ROCKET — Land an Apollo Capsule on the Moon";
            Console.WriteLine("ROCKET — A Computer Simulation of an Apollo Lunar Landing Capsule\n");
            Console.WriteLine("The on-board computer has failed — you must land manually.");
            Console.WriteLine("Set the burn rate of the retro rockets every 10 seconds.");
            Console.WriteLine("Valid burn is 0..200 lb/s (1..7 has no effect).");
            Console.WriteLine($"Capsule weight: {CapsuleWeight:N0} lb     Fuel weight: {StartFuel:N0} lb");
            Console.WriteLine("Good luck!\n");

            // State (feet/second, feet, seconds)
            double t = 0.0;
            double v = MphToFps(StartVelMPH);        // ft/s (downward positive)
            double h = MilesToFeet(StartAltMiles);   // feet above surface
            double fuel = StartFuel;

            // For display
            PrintHeader();
            PrintRow(t, h, v, fuel, 0);

            double lastBurn = 0;

            while (h > 0)
            {
                // Ask player for burn this interval
                double requested = PromptBurn(lastBurn);
                lastBurn = requested;

                // Enforce ignition threshold and limits
                double effectiveBurn = (requested < IgnitionThreshold) ? 0 : Math.Min(requested, MaxBurn);

                // Clamp for fuel availability across 10 seconds
                double maxBurnWithFuel = fuel / Dt; // lb/s we can sustain for full interval
                if (effectiveBurn > maxBurnWithFuel && fuel > 0)
                {
                    // We'll burn what's left; this shortens the effective thrust time
                    effectiveBurn = maxBurnWithFuel;
                }

                // Compute thrust acceleration for this interval (ft/s^2, upward negative on v)
                double aThrust = (effectiveBurn / MaxBurn) * ThrustAtMax; // upward decel
                double aNet = G - aThrust; // positive means accelerating downward

                // If we will hit the surface during this interval, solve the exact impact time
                // h(tau) = h - v*tau - 0.5*aNet*tau^2   (remember downward v is positive; altitude decreases)
                // Find tau > 0 where h(tau)=0
                double tau = Dt;
                if (WillCrossSurface(h, v, aNet, Dt))
                {
                    tau = SolveImpactTime(h, v, aNet);
                }

                // Semi-implicit Euler using the interval (or the exact impact time)
                v += aNet * tau;
                h -= v * tau; // h decreases by downward motion; we used the new v (works fine for small dt)

                // Fuel consumption (proportional to burn time actually used)
                double fuelUsed = effectiveBurn * tau;
                fuel = Math.Max(0, fuel - fuelUsed);

                t += tau;

                // Display status row (altitude cannot be negative)
                PrintRow(t, Math.Max(0, h), v, fuel, requested);

                if (h <= 0) break; // we landed/crashed within this tau
            }

            // Final outcome
            double impactFps = Math.Abs(v); // ft/s
            double impactMph = FpsToMph(impactFps);
            Console.WriteLine($"\nON MOON AT {t:F3} SEC — IMPACT VELOCITY {impactMph:F4} MPH");

            // Grade the landing using ft/s thresholds
            string verdict =
                impactFps <= 1.0 ? "PERFECT LANDING!" :
                impactFps <= 10.0 ? "Good landing (could be better)." :
                impactFps <= 30.0 ? "Craft damaged... you survive but need repairs." :
                impactFps <= 60.0 ? "Hard impact — severe damage; doubtful you survive." :
                "CRASH — total destruction.";

            Console.WriteLine(verdict);
            Console.WriteLine("\nTry again?");
        }

        static bool WillCrossSurface(double h, double v, double a, double dt)
        {
            // Predict altitude after dt using constant acceleration
            // h' = h - v*dt - 0.5*a*dt^2  (downward is positive v)
            double hNext = h - v * dt - 0.5 * a * dt * dt;
            return hNext <= 0;
        }

        static double SolveImpactTime(double h, double v, double a)
        {
            // Solve 0 = h - v*t - 0.5*a*t^2  ->  0.5*a*t^2 + v*t - h = 0
            double A = 0.5 * a;
            double B = v;
            double C = -h;
            if (Math.Abs(A) < 1e-9)
            {
                // Linear: v*t - h = 0
                return Math.Max(0, h / v);
            }
            double disc = B * B - 4 * A * C;
            if (disc < 0) disc = 0;
            double t1 = (-B + Math.Sqrt(disc)) / (2 * A);
            double t2 = (-B - Math.Sqrt(disc)) / (2 * A);
            // We want the small positive root
            double tau = double.PositiveInfinity;
            if (t1 > 0) tau = Math.Min(tau, t1);
            if (t2 > 0) tau = Math.Min(tau, t2);
            return double.IsInfinity(tau) ? 0 : tau;
        }

        // ---- UI helpers ----
        static void PrintHeader()
        {
            Console.WriteLine();
            Console.WriteLine("   SEC |     MI + FT     |    MPH    |  LB FUEL | BURN RATE");
            Console.WriteLine("-------+------------------+-----------+----------+----------");
        }

        static void PrintRow(double t, double hFeet, double vFps, double fuel, double burnAsked)
        {
            // Format altitude as MI + FT (feet shown as integer 0..5279)
            double miles = Math.Floor(hFeet / 5280.0);
            double feet = Math.Max(0, hFeet - miles * 5280.0);

            double mph = FpsToMph(Math.Abs(vFps));
            Console.WriteLine(
                $"{t,6:F0} | {miles,4:0} {feet,5:0}  | {mph,9:F1} | {fuel,8:0} | {burnAsked,9:0}");
        }

        static double PromptBurn(double last)
        {
            while (true)
            {
                Console.Write($"BURN RATE (0..200 lb/s) [last {last:0}]: ");
                var s = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(s)) return last; // repeat last choice
                if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var x))
                {
                    if (x < 0) x = 0;
                    if (x > MaxBurn) x = MaxBurn;
                    return x;
                }
                Console.WriteLine("Please enter a number between 0 and 200.");
            }
        }

        // ---- Unit helpers ----
        static double MilesToFeet(double mi) => mi * 5280.0;
        static double FpsToMph(double fps) => fps * 3600.0 / 5280.0;
        static double MphToFps(double mph) => mph * 5280.0 / 3600.0;
    }
}
