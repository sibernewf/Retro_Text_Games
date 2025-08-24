using System;
using System.Globalization;

namespace Rocket2
{
    class Program
    {
        static void Main()
        {
            Console.Title = "ROCKET2 — Lunar Lander (WASD not needed here 🙂)";
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            do
            {
                RunOneGame();
            }
            while (AskYesNo("\nDO YOU WANT TO FLY IT AGAIN ? (YES OR NO) "));
        }

        // --------------------------- GAME LOOP ----------------------------
        static void RunOneGame()
        {
            PrintHeader();

            bool flownApollo = AskYesNo("HAVE YOU FLOWN ON AN APOLLO/LEM MISSION BEFORE (YES OR NO)? ");
            int measure = AskMeasureSystem();

            // Unit system
            var units = (measure == 1)
                ? UnitSystem.English()   // feet, ft/s, lb, lbf
                : UnitSystem.Metric();   // meters, m/s, kg, N

            // Show short/long/no instructions (like the BASIC offered)
            ShowInstructionsMenu(units);

            // Initial conditions (tuned for a fun-but-manageable approach).
            // These are not exact Apollo numbers; they’re balanced for gameplay.
            var s = new State(units)
            {
                Time = 0,
                Altitude = 1500 * units.Meter,     // 1500 m (or converted to ft)
                Vz = -40 * units.Mps,              // descending ~40 m/s (≈ 131 ft/s)
                Vx = 25 * units.Mps,               // a bit of horizontal drift
                Fuel = 900 * units.Kg,             // fuel mass
                DryMass = 1500 * units.Kg,         // dry mass
                Isp = 305,                         // s (LM descent Isp ~ 311; round to 305)
                ThrustMax = 45_000 * units.Newton, // ~45 kN; tweakable
                Gravity = 1.62 * units.Mps2,       // Moon gravity
            };

            Console.WriteLine();
            Console.WriteLine("INPUT:  TIME INTERVAL IN SECONDS  (T)");
            Console.WriteLine("        PERCENTAGE OF THRUST      (P)");
            Console.WriteLine("        ATTITUDE ANGLE IN DEGREES (A)");
            Console.WriteLine();
            Console.WriteLine("ALL ANGLES BETWEEN -180 AND +180 DEGREES ARE ACCEPTED.");
            Console.WriteLine("FULL PERCENT = 1 SEC AT MAX THRUST (APPROX. ROCKET EQUATION).");
            Console.WriteLine("NEGATIVE THRUST (P<0) IS PROHIBITED.");
            Console.WriteLine("AVAILABLE ENGINE POWER: 0 (ZERO) TO 100 (PERCENT).");
            Console.WriteLine("TO ABORT THE MISSION AT ANY TIME, ENTER 0,0,0\n");

            PrintStepHeader(units);

            // simulation loop
            while (true)
            {
                // If out of fuel, force P=0
                if (s.Fuel <= 0) s.Fuel = 0;

                var (T, P, Adeg, aborted) = ReadCommandTriple(units);
                if (aborted)
                {
                    Console.WriteLine("\nMISSION ABORTED");
                    return;
                }

                P = Math.Clamp(P, 0, 100);

                // Integrate one step
                var result = IntegrateStep(s, T, P, Adeg);

                PrintStepRow(units, s);

                // Check surface contact
                if (s.Altitude <= 0)
                {
                    // Compute landing metrics
                    double vVert = s.Vz;                       // vertical rate (+up, -down)
                    double vHor = Math.Abs(s.Vx);
                    double impactSpeed = Math.Sqrt(vHor * vHor + vVert * vVert);

                    // Safety thresholds (tune freely)
                    double safeVVert = 2 * units.Mps;          // ≤ ~2 m/s vertical
                    double safeVHor = 1 * units.Mps;           // ≤ ~1 m/s horizontal

                    if (-vVert <= safeVVert && vHor <= safeVHor)
                    {
                        Console.WriteLine();
                        Console.WriteLine("TRANQUILLITY BASE HERE -- THE EAGLE HAS LANDED");
                        Console.WriteLine("CONGRATULATIONS -- THERE WAS NO SPACECRAFT DAMAGE.");
                        return;
                    }
                    else
                    {
                        Console.WriteLine();
                        Console.WriteLine("CRASH !!!!!!!!!!!!!");
                        double craterDepth = Math.Max(0, impactSpeed * 0.2); // just a fun metric
                        Console.WriteLine($"YOUR IMPACT CREATED A CRATER {craterDepth / units.Meter:0.00} {units.MName} DEEP");
                        Console.WriteLine($"AT CONTACT YOU WERE TRAVELLING {impactSpeed / units.Mps:0.00} {units.VName}.");
                        return;
                    }
                }

                // Out of fuel message (only once)
                if (!s.OutOfFuelAnnounced && s.Fuel <= 0)
                {
                    s.OutOfFuelAnnounced = true;
                    Console.WriteLine("* * * YOU ARE OUT OF FUEL * * *");
                }
            }
        }

        // --------------------------- PHYSICS -----------------------------
        // Simple 2D model: thrust vector at attitude A (deg).
        // A=0° = straight up (against gravity); positive rotates toward +X (to the right).
        // Rocket equation: mDot = T/(Isp*g0). Fuel decreases: m_fuel -= mDot * dt.
        // Accel = Thrust/m - gravity (vertical component) applied over dt.
        static StepResult IntegrateStep(State s, double T, double P, double Adeg)
        {
            double dt = Math.Max(0, T);
            if (dt == 0) return new StepResult();

            double thrust = s.ThrustMax * (P / 100.0);
            if (s.Fuel <= 0) thrust = 0;

            // mass flow (kg/s or slugs/s depending on unit pack), but we keep it consistent within UnitSystem
            const double g0 = 9.80665; // m/s^2 reference; unit conversion handled in UnitSystem scaling
            double mdot_SI = thrust / (s.Isp * g0); // this is in kg/s-equivalent
            // convert mdot into our unit system “mass” by using the system's Kg scale (keeps numbers consistent)
            double mdot = mdot_SI * s.Units.Kg;

            double fuelNeeded = mdot * dt;
            if (fuelNeeded > s.Fuel)
            {
                // scale down thrust/time proportionally to remaining fuel
                double fraction = s.Fuel / Math.Max(1e-9, fuelNeeded);
                thrust *= fraction;
                dt *= fraction;
                mdot *= fraction;
                fuelNeeded = s.Fuel; // now exact
            }

            double mass = s.DryMass + s.Fuel;
            double ax = 0, az = 0;

            if (thrust > 0 && mass > 0)
            {
                double aRad = Adeg * Math.PI / 180.0;
                double tx = thrust * Math.Sin(aRad);
                double tz = thrust * Math.Cos(aRad); // up component

                ax = tx / mass;
                az = tz / mass - s.Gravity;
            }
            else
            {
                ax = 0;
                az = -s.Gravity;
            }

            // Integrate (semi-implicit Euler)
            s.Vx += ax * dt;
            s.Vz += az * dt;
            s.X  += s.Vx * dt;
            s.Altitude += s.Vz * dt;

            // Consume fuel
            s.Fuel = Math.Max(0, s.Fuel - fuelNeeded);

            s.Time += dt;

            return new StepResult { UsedTime = dt, UsedFuel = fuelNeeded };
        }

        // --------------------------- I/O & UI ---------------------------
        static void PrintHeader()
        {
            Console.WriteLine("LUNAR LANDING SIMULATION (ROCKET2)");
            Console.WriteLine("----------------------------------");
        }

        static int AskMeasureSystem()
        {
            Console.WriteLine();
            Console.WriteLine("WHICH SYSTEM OF MEASUREMENT DO YOU PREFER ?");
            Console.WriteLine("  1) ENGLISH");
            Console.WriteLine("  2) METRIC");
            int n = AskInt("ENTER THE APPROPRIATE NUMBER: ", 1, 2);
            Console.WriteLine();
            return n;
        }

        static void ShowInstructionsMenu(UnitSystem u)
        {
            Console.WriteLine("DO YOU WANT THE COMPLETE INSTRUCTIONS OR THE OUTPUT-ONLY STATEMENTS?");
            Console.WriteLine("  1) OUTPUT STATEMENTS ONLY");
            Console.WriteLine("  2) INCOMPLETE INSTRUCTIONS");
            Console.WriteLine("  3) COMPLETE INSTRUCTIONS");
            int n = AskInt("> ", 1, 3);

            if (n == 1) return;

            Console.WriteLine();
            Console.WriteLine("YOU ARE ON A LUNAR LANDING MISSION, AS THE PILOT OF");
            Console.WriteLine("THE LUNAR EXCURSION MODULE. YOU WILL BE EXPECTED TO");
            Console.WriteLine("GIVE CERTAIN COMMANDS TO THE MODULE NAVIGATION SYSTEM.");
            Console.WriteLine("THE ON-BOARD COMPUTER WILL GIVE A RUNNING ACCOUNT");
            Console.WriteLine("OF INFORMATION NEEDED TO NAVIGATE THE SHIP.\n");

            Console.WriteLine("AT EACH STEP, ENTER:");
            Console.WriteLine("  T  = TIME INTERVAL (SECONDS)");
            Console.WriteLine("  P  = THRUST PERCENT (0..100)");
            Console.WriteLine("  A  = ATTITUDE ANGLE (DEGREES)");
            Console.WriteLine("      (0° points thrust straight up; +A yaws to the right)");
            Console.WriteLine("FUEL BURN IS APPROXIMATE VIA THE ROCKET EQUATION (Isp).\n");

            if (n == 3)
            {
                Console.WriteLine("SAFE LANDING GUIDELINE:");
                Console.WriteLine($"  |VERTICAL RATE| ≤ {2 / u.Mps:0.0} {u.VName}");
                Console.WriteLine($"  |HORIZONTAL RATE| ≤ {1 / u.Mps:0.0} {u.VName}");
                Console.WriteLine();
            }
        }

        static void PrintStepHeader(UnitSystem u)
        {
            Console.WriteLine($"{"t",8} {"ALT",10} {"V-vert",10} {"V-horz",10} {"FUEL",10}");
            Console.WriteLine($"{"(s)",8} {($"({u.MName})"),10} {($"({u.VName})"),10} {($"({u.VName})"),10} {($"({u.MassName})"),10}");
        }

        static void PrintStepRow(UnitSystem u, State s)
        {
            Console.WriteLine($"{s.Time,8:0} {s.Altitude/u.M:10:0.0} {s.Vz/u.V:10:0.00} {s.Vx/u.V:10:0.00} {s.Fuel/u.Mass:10:0.0}");
        }

        static (double T, double P, double Adeg, bool aborted) ReadCommandTriple(UnitSystem u)
        {
            while (true)
            {
                Console.Write("\nT,P,A ? ");
                string? line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split(new[] { ',', ' ', '\t', ';' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 3) { Console.WriteLine("PLEASE ENTER THREE VALUES SEPARATED BY COMMAS OR SPACES."); continue; }

                if (!double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out double T)) { Console.WriteLine("BAD T."); continue; }
                if (!double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double P)) { Console.WriteLine("BAD P."); continue; }
                if (!double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out double A)) { Console.WriteLine("BAD A."); continue; }

                if (T == 0 && P == 0 && A == 0) return (0, 0, 0, true);
                if (P < 0) { Console.WriteLine("NEGATIVE THRUST VALUE — PROHIBITED."); P = 0; }

                return (T, P, A, false);
            }
        }

        static bool AskYesNo(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string? s = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(s)) continue;
                s = s.Trim().ToUpperInvariant();
                if (s.StartsWith("Y")) return true;
                if (s.StartsWith("N")) return false;
                Console.WriteLine("PLEASE ANSWER YES OR NO.");
            }
        }

        static int AskInt(string prompt, int min, int max)
        {
            while (true)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine(), out int v) && v >= min && v <= max) return v;
                Console.WriteLine($"ENTER A NUMBER BETWEEN {min} AND {max}.");
            }
        }
    }

    // --------------------------- DATA TYPES ------------------------------
    sealed class State
    {
        public readonly UnitSystem Units;
        public State(UnitSystem u) { Units = u; }

        public double Time;

        // Kinematics (z = up)
        public double Altitude;   // z
        public double Vz;         // vertical velocity (+up)
        public double X;          // horizontal distance (optional)
        public double Vx;         // horizontal velocity

        // Mass/engine
        public double Fuel;       // remaining fuel mass
        public double DryMass;    // dry mass
        public double Isp;        // specific impulse (s)
        public double ThrustMax;  // maximum thrust (force)
        public double Gravity;    // lunar gravity magnitude

        public bool OutOfFuelAnnounced = false;
    }

    struct StepResult
    {
        public double UsedTime;
        public double UsedFuel;
    }

    // Simple unit pack so one code path can print English or Metric nicely
    // Simple unit pack so one code path can print English or Metric nicely
sealed class UnitSystem
{
    // Base scales
    public double Meter  { get; init; }    // length unit → internal length
    public double Mps    { get; init; }    // velocity unit → internal velocity
    public double Kg     { get; init; }    // mass unit → internal mass
    public double Newton { get; init; }    // force unit → internal force
    public double Mps2   { get; init; }    // accel unit → internal accel

    // For printing (defaults silence CS8618)
    public string MName    { get; init; } = "m";
    public string VName    { get; init; } = "m/s";
    public string MassName { get; init; } = "kg";

    // Helpers to convert back when printing
    public double M    => Meter == 0 ? 1 : Meter;
    public double V    => Mps   == 0 ? 1 : Mps;
    public double Mass => Kg    == 0 ? 1 : Kg;

    public static UnitSystem Metric() => new UnitSystem
    {
        Meter = 1,
        Mps = 1,
        Kg = 1,
        Newton = 1,
        Mps2 = 1,
        MName = "m",
        VName = "m/s",
        MassName = "kg"
    };

    public static UnitSystem English() => new UnitSystem
    {
        // 1 m = 3.28084 ft
        Meter = 3.28083989501312,
        Mps = 3.28083989501312,
        Kg = 2.20462262185,     // 1 kg ≈ 2.205 lbm
        Newton = 0.224808943,   // 1 N ≈ 0.2248 lbf
        Mps2 = 3.28083989501312,
        MName = "ft",
        VName = "ft/s",
        MassName = "lb"
    };
}
}
