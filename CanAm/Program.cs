using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace CanAm
{
    enum SegKind { Straight, Curve }

    sealed class Segment
    {
        public string Name { get; }
        public SegKind Kind { get; }
        public double MaxMph { get; }          // hard limit (course sign)
        public double LengthMi { get; }        // miles
        public Segment(string name, SegKind k, double max, double len)
        { Name = name; Kind = k; MaxMph = max; LengthMi = len; }
    }

    sealed class Driver
    {
        public string Name { get; }
        public bool Human { get; }
        public double Adhesion;                // lower = grippier (like original)
        public double CarOffset;               // car strength mph bonus (small)
        public bool Alive = true;
        public double TimeSec = 0;
        public double LastSpeed = 0;
        public double DraftCredit = 0;         // small time credit next section
        public Driver(string name, bool human, double adhesion, double offset)
        { Name = name; Human = human; Adhesion = adhesion; CarOffset = offset; }
    }

    static class Program
    {
        static readonly Random Rng = new();
        static readonly List<string> log = new();

        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            LogLine("CAN-AM — Canadian-American Challenge Cup (modernized)");
            LogLine("Speeds in MPH. Enter repeats last choice. Type Q to quit.\n");

            var course = BuildCourse();
            var drivers = SetupDrivers();

            // race-long hazard state
            bool rain = false;
            double oil = 0.0;                  // 0..1; grows with incidents, fades slowly

            LogLine($"Track length ~ {course.Sum(s => s.LengthMi):0.00} miles, {course.Count} sections.");
            LogLine("");

            // main race loop
            for (int si = 0; si < course.Count; si++)
            {
                var seg = course[si];
                LogLine($"\n=== Section {si + 1}/{course.Count}: {seg.Name} — {seg.Kind} — {seg.LengthMi:0.00} mi — Max {seg.MaxMph:0} mph ===");

                // Hazards may change a bit each section
                (rain, oil) = UpdateHazards(rain, oil, seg);
                if (rain) LogLine("Weather: RAIN — grip reduced.");
                if (oil > 0.08) LogLine($"Track: OIL on surface (level {oil:0.00}).");

                // For time ordering within section we snapshot current standings
                var order = drivers
                    .Select(d => new { D = d, t = d.TimeSec })
                    .OrderBy(x => x.t)
                    .Select(x => x.D)
                    .ToList();

                foreach (var d in order)
                {
                    if (!d.Alive) continue;

                    // compute a recommended safe speed based on adhesion, hazards, and geometry
                    double safe = SafeSpeed(seg, d, rain, oil);

                    double choice = d.Human
                        ? PromptSpeed(d, seg, safe)
                        : AutoSpeed(d, seg, safe);

                    if (choice < 0) Quit();

                    // clamp against posted max
                    choice = Math.Clamp(choice, 0, seg.MaxMph + d.CarOffset);

                    // compute wipe probability if above safe
                    double pWipe = CrashProbability(choice, safe, seg, rain, oil, d);

                    bool wiped = Rng.NextDouble() < pWipe;
                    if (wiped)
                    {
                        d.Alive = false;
                        LogLine($"{d.Name} WIPES OUT on {seg.Name} at {choice:0} mph — DNF.");
                        oil = Math.Min(1.0, oil + 0.10); // accidents make oil worse
                        continue;
                    }

                    // segment time (hours = miles/mph)
                    double hours = seg.LengthMi / Math.Max(5, choice);
                    double sec = hours * 3600.0;

                    // apply drafting credit from previous gap
                    if (d.DraftCredit > 0)
                    {
                        double credit = Math.Min(sec * 0.02, d.DraftCredit); // up to 2% or given credit
                        sec -= credit;
                        LogLine($"{d.Name} benefits from drafting (−{credit:0.00}s).");
                        d.DraftCredit = 0;
                    }

                    d.TimeSec += sec;
                    d.LastSpeed = choice;
                    LogLine($"{d.Name} runs {choice:0} mph → +{sec:0.00}s (Total {d.TimeSec:0.00}s).");
                }

                // after everyone ran the section, compute new drafting credits:
                // if you’re within 1.0 s behind someone, carry a small credit (0.5s) to next section
                var alive = drivers.Where(x => x.Alive).OrderBy(x => x.TimeSec).ToList();
                for (int i = 1; i < alive.Count; i++)
                {
                    double gap = alive[i].TimeSec - alive[i - 1].TimeSec;
                    if (gap <= 1.0) alive[i].DraftCredit = 0.5;
                }
            }

            LogLine("\n===== CHECKERED FLAG =====");
            var ranked = drivers
                .OrderBy(d => d.Alive ? 0 : 1)
                .ThenBy(d => d.TimeSec)
                .ToList();

            int place = 1;
            foreach (var d in ranked)
            {
                string status = d.Alive ? $"{d.TimeSec:0.00} s" : "DNF";
                LogLine($"{place,2}. {d.Name,-12}  {status}");
                if (d.Alive) place++;
            }

            // write file
            var path = Path.GetFullPath("canam_results.txt");
            File.WriteAllText(path, string.Join(Environment.NewLine, log), Encoding.UTF8);
            Console.WriteLine($"\nPlay-by-play saved to: {path}");
        }

        // ===== course & setup =====
        static List<Segment> BuildCourse() => new()
        {
            // Based on the listing’s “working portion”; lengths are as printed (mi), max speeds as posted
            new("Straight A", SegKind.Straight, 200, 0.30),
            new("Curve 1",    SegKind.Curve,    125, 0.10),
            new("Straight B", SegKind.Straight, 200, 0.17),
            new("Curve 2",    SegKind.Curve,    125, 0.10),
            new("Straight C", SegKind.Straight, 200, 0.15),
            new("Curve 3",    SegKind.Curve,    150, 0.12),
            new("Straight D", SegKind.Straight, 200, 0.35),
            new("Curve 4",    SegKind.Curve,    125, 0.10),
            new("Straight E", SegKind.Straight, 200, 0.25),
            new("Straight F", SegKind.Straight, 200, 0.22),
            new("Straight G", SegKind.Straight, 200, 0.45),
            new("Straight H", SegKind.Straight, 200, 0.42),
            new("Curve 7",    SegKind.Curve,    125, 0.10),
            new("Curve 8",    SegKind.Curve,    150, 0.15),
            new("Straight I", SegKind.Straight, 200, 0.70),
            new("Start/Finish (Straight J)", SegKind.Straight, 150, 0.32)
        };

        static List<Driver> SetupDrivers()
        {
            int n = AskInt("How many drivers (1–8)? ", 1, 8);
            int humans = AskInt("How many human drivers (0–8)? ", 0, n);

            var drivers = new List<Driver>();

            for (int i = 1; i <= humans; i++)
            {
                Console.Write($"Name of driver {i}: ");
                string? raw = Console.ReadLine();
                if (raw != null && raw.Trim().Equals("Q", StringComparison.OrdinalIgnoreCase)) Quit();
                string name = string.IsNullOrWhiteSpace(raw) ? $"Driver{i}" : raw!.Trim();
                drivers.Add(new Driver(name, human: true, adhesion: Adhesion(), offset: CarOffset()));
            }
            for (int i = humans + 1; i <= n; i++)
            {
                string name = AutoPilotName(i - humans);
                drivers.Add(new Driver(name, human: false, adhesion: Adhesion(), offset: CarOffset()));
            }

            LogLine("\nDrivers and adhesion factors (lower = grippier):");
            foreach (var d in drivers)
                LogLine($"  {d.Name,-12}  adhesion={d.Adhesion:0.000}  carΔ={d.CarOffset:+0.0;-0.0;0}");

            return drivers;
        }

        static string AutoPilotName(int i)
        {
            string[] pool = { "McLaren", "Lola", "Penske", "Chaparral", "Hulme", "Surtees", "Andretti", "Gurney" };
            return pool[(i - 1) % pool.Length] + (i > pool.Length ? $" #{i}" : "");
        }

        static double Adhesion()
        {
            // Original prints adhesion as KA*10^-5; lower better.
            // We’ll generate between 0.70 and 1.05 (grippy to slippery).
            return 0.70 + Rng.NextDouble() * 0.35;
        }
        static double CarOffset() => Rng.NextDouble() * 6 - 3; // -3..+3 mph

        // ===== hazards =====
        static (bool rain, double oil) UpdateHazards(bool rain, double oil, Segment s)
        {
            // Small chance for rain to start/stop; rain fades rarely
            if (!rain && Rng.NextDouble() < 0.07) rain = true;
            else if (rain && Rng.NextDouble() < 0.06) rain = false;

            // Oil grows when it’s already present, slowly diffuses otherwise
            oil = Math.Clamp(oil + (oil > 0 ? 0.01 : 0.0) - 0.005, 0, 0.25);

            // Curves hold oil effects more
            if (s.Kind == SegKind.Curve) oil = Math.Min(0.30, oil + 0.01);
            return (rain, oil);
        }

        // ===== physics-ish helpers =====
        static double SafeSpeed(Segment seg, Driver d, bool rain, double oil)
        {
            // Baseline safe speed from max and adhesion factor
            double baseSafe = seg.Kind == SegKind.Straight
                ? seg.MaxMph * (1.00 - 0.10 * (d.Adhesion - 0.70)) // grippy cars can approach max
                : seg.MaxMph * (0.85 - 0.30 * (d.Adhesion - 0.70)); // curves punish adhesion

            // Weather/hazards reduce safe speed
            if (rain) baseSafe *= 0.90;
            if (oil > 0) baseSafe *= Math.Max(0.75, 1.0 - oil * 0.8);

            // Car strength bonus
            baseSafe += d.CarOffset * 0.5;

            // keep within posted + a little headroom
            return Math.Clamp(baseSafe, 40, seg.MaxMph + 10);
        }

        static double CrashProbability(double speed, double safe, Segment seg, bool rain, double oil, Driver d)
        {
            if (speed <= safe) return 0.01 * (seg.Kind == SegKind.Straight ? 0.5 : 1.0); // tiny base risk

            double over = (speed - safe) / Math.Max(20, safe); // over-speed fraction
            double p = 0.03 + over * over * (seg.Kind == SegKind.Straight ? 0.25 : 0.45);
            if (rain) p += 0.05;
            p += oil * 0.6;
            // slippery car more likely to lose it
            p *= (0.9 + (d.Adhesion - 0.70));
            return Math.Clamp(p, 0.01, 0.90);
        }

        // ===== input / AI =====
        static double PromptSpeed(Driver d, Segment s, double safe)
        {
            while (true)
            {
                Console.Write($"{d.Name} — {s.Name} (safe≈{safe:0}, max {s.MaxMph:0}) mph: ");
                string? raw = Console.ReadLine();
                if (raw is null) return -1;
                raw = raw.Trim();
                if (raw.Equals("Q", StringComparison.OrdinalIgnoreCase)) return -1;
                if (raw == "" && d.LastSpeed > 0) return d.LastSpeed;
                if (double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out double v))
                    return v;
            }
        }

        static double AutoSpeed(Driver d, Segment s, double safe)
        {
            // temperament: some drivers are brave, some cautious
            double mood = Rng.NextDouble();
            double target = mood switch
            {
                < 0.25 => safe * (s.Kind == SegKind.Curve ? 0.92 : 0.96),
                < 0.75 => safe * (s.Kind == SegKind.Curve ? 1.00 : 1.03),
                _      => safe * (s.Kind == SegKind.Curve ? 1.05 : 1.08),
            };
            // clamp around posted
            target = Math.Min(target + d.CarOffset, s.MaxMph + d.CarOffset);
            // add tiny noise
            target += Rng.NextDouble() * 2 - 1;
            target = Math.Max(40, target);
            LogLine($"{d.Name} targets {target:0} mph.");
            return target;
        }

        // ===== utilities =====
        static int AskInt(string prompt, int lo, int hi)
        {
            while (true)
            {
                Console.Write(prompt);
                var s = Console.ReadLine();
                if (s != null && s.Trim().Equals("Q", StringComparison.OrdinalIgnoreCase)) Quit();
                if (int.TryParse(s, out int v) && v >= lo && v <= hi) return v;
            }
        }

        static void Quit()
        {
            Console.WriteLine("Quitting…");
            Environment.Exit(0);
        }

        static void LogLine(string s)
        {
            Console.WriteLine(s);
            log.Add(s);
        }
    }
}
