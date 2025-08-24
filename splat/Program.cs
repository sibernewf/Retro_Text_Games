using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

class Program
{
    // Save file of successful jumps (same spirit as the book)
    const string SaveFile = "PARACH.UTE";

    // Planetary gravities (ft/s^2) — rounded, good enough for a game
    static readonly (string name, double g)[] Bodies =
    {
        ("Mercury",   12.17), // 3.7 m/s^2
        ("Venus",     28.03), // 8.53
        ("Earth",     32.17), // 9.81
        ("Moon",       5.31), // 1.62
        ("Mars",      12.11), // 3.69
        ("Jupiter",   84.25), // 25.7
        ("Saturn",    35.12), // 10.7
        ("Uranus",    29.04), // 8.85
        ("Neptune",   36.59), // 11.15
        ("Sun",      896.52)  // 273.9 (ridiculous but fun)
    };

    static void Main()
    {
        Console.Title = "SPLAT — Open a Parachute at the Last Moment";
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        while (true)
        {
            Console.Clear();
            PrintBanner();
            Console.WriteLine("[P]lay   [H]elp   [Q]uit");
            var k = ReadKey("Choose: ", "PHQ");
            if (k == 'Q') return;
            if (k == 'H') { ShowHelp(); continue; }
            PlayOneRound();
        }
    }

    static void PlayOneRound()
    {
        var rng = new Random();

        Console.Clear();
        PrintBanner();

        // Terminal velocity
        double vt; // ft/s
        if (AskYesNo("SELECT YOUR OWN TERMINAL VELOCITY (YES OR NO)? "))
        {
            vt = AskDouble("OK. TERMINAL VELOCITY (ft/s)? ", 60, 350);
        }
        else
        {
            // human belly-to-earth typical ~176 ft/s (≈ 120 mph)
            vt = 160 + rng.NextDouble() * 40; // 160..200
            Console.WriteLine($"FINE. I'LL USE VT = {vt:F2} FT/SEC.");
        }

        // Gravity
        double g;
        string body;
        if (AskYesNo("WANT TO SELECT ACCELERATION DUE TO GRAVITY (YES OR NO)? "))
        {
            g = AskDouble("ACCELERATION (ft/s^2)? ", 1, 1200);
            body = "CUSTOM";
        }
        else
        {
            var p = Bodies[rng.Next(Bodies.Length)];
            body = p.name;
            g = p.g;
            Console.WriteLine($"FINE. YOU'RE ON {body.ToUpperInvariant()}; ACCELERATION = {g:F2} FT/SEC/SEC.");
        }

        // Altitude (pick generous but not absurd; scale with body a bit)
        // Earth-ish skydives ~12k–18k ft; the BASIC sample showed ~99k ft.
        // We'll go 5k..100k with mild bias toward higher on low-g bodies.
        double baseMin = 5000, baseMax = 100000;
        double bias = Math.Clamp(32.17 / g, 0.2, 2.0); // low g -> bigger bias
        double altitude = baseMin + (baseMax - baseMin) * Math.Pow(new Random().NextDouble(), 1 / bias);
        altitude = Math.Round(altitude);

        Console.WriteLine();
        Console.WriteLine($"ALTITUDE        = {altitude:F0} FT");
        Console.WriteLine($"TERM-VELOCITY   = {vt:F2} FT/SEC");
        Console.WriteLine($"ACCELERATION    = {g:F2} FT/SEC/SEC");
        Console.WriteLine();

        // Freefall time
        double tff = AskDouble("SET THE TIMER FOR YOUR FREEFALL — HOW MANY SECONDS? ", 0.25, 300);

        Console.WriteLine("\nHERE WE GO.\n");
        PrintProgressTable(altitude, g, vt, tff);

        // Determine outcome
        double fallenT = Fallen(g, vt, tff);
        bool splat = fallenT >= altitude - 1e-6;

        if (splat)
        {
            Console.WriteLine("\nS P L A T");
            Console.WriteLine("MAY THE ANGELS (OR HEAVEN) LEAD YOU INTO PARADISE!  I'LL GIVE YOU ANOTHER CHANCE.");
        }
        else
        {
            double heightLeft = altitude - fallenT; // opening height
            Console.WriteLine("\nCHUTE OPEN");
            Console.WriteLine($"CONGRATULATIONS! YOU LIVED. YOU OPENED AT ~{heightLeft:F1} FT.");
            // Record and rank
            int rank = RecordSuccessfulJump(heightLeft);
            if (rank > 0)
                Console.WriteLine($"CONSERVATIVE? A JUMP YOU RANKED ONLY {rank} IN THE HALL OF BRAVERY.");
            else
                Console.WriteLine("FIRST SUCCESSFUL JUMP ON RECORD. YOU SET THE BAR!");
        }

        Console.WriteLine();
        var again = AskYesNo("DO YOU WANT TO PLAY AGAIN? ");
        if (!again) Environment.Exit(0);
    }

    // -------- Physics (linear drag toward terminal velocity) --------
    // v(t) = Vt * (1 - e^(-g t / Vt))
    // s(t) = Vt * t - (Vt^2 / g) * (1 - e^(-g t / Vt))
    static double Fallen(double g, double vt, double t)
    {
        double a = g / vt;
        return vt * t - (vt * vt / g) * (1 - Math.Exp(-a * t));
    }

    static void PrintProgressTable(double altitude, double g, double vt, double tff)
    {
        Console.WriteLine("TIME (SEC)    DIST TO FALL (FT)");
        Console.WriteLine("==========    =================");
        int rows = 8;
        double dt = tff / rows;
        for (int i = 0; i <= rows; i++)
        {
            double t = Math.Round(i * dt, 2);
            double fallen = Fallen(g, vt, t);
            double remain = Math.Max(0, altitude - fallen);
            Console.WriteLine($"{t,8:0.##}    {remain,14:0.##}");
            if (remain <= 0)
            {
                Console.WriteLine("                 S P L A T");
                break;
            }
        }
    }

    // -------- Score file handling --------
    // Store successful jumps as "heightFeet" one per line; sort ascending (braver first)
    static int RecordSuccessfulJump(double openHeight)
    {
        try
        {
            var list = new List<double>();
            if (File.Exists(SaveFile))
            {
                foreach (var line in File.ReadAllLines(SaveFile))
                    if (double.TryParse(line.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var v))
                        list.Add(v);
            }
            list.Add(openHeight);

            // Rank (1-based)
            list.Sort();
            int rank = list.FindIndex(v => v.Equals(openHeight)) + 1;

            // Save top 100 for sanity
            File.WriteAllLines(SaveFile, list.Take(100).Select(v => v.ToString("0.###", CultureInfo.InvariantCulture)));
            return rank;
        }
        catch
        {
            // If file ops fail, just skip ranking
            return 0;
        }
    }

    // -------- Retro Help --------
    static void ShowHelp()
    {
        Console.Clear();
        DrawBox("SPLAT — HOW TO PLAY", new[]
        {
            "Objective:",
            "  Open your parachute at the latest safe moment.",
            "",
            "Setup:",
            "  • Choose your own terminal velocity (ft/s), or let the game pick a typical value.",
            "  • Choose gravity (ft/s²), or let the game pick a random body (Mercury…Neptune, Moon, Sun).",
            "  • The game sets a jump altitude.",
            "",
            "Play:",
            "  • Enter your free-fall time (seconds).",
            "  • The game prints 8 progress checkpoints: time vs. remaining distance.",
            "  • If distance reaches 0 before your time is up: SPLAT.",
            "  • If you survive, your opening height is recorded to PARACH.UTE.",
            "  • Rankings prefer LOWER opening heights (braver = better).",
            "",
            "Physics:",
            "  Uses linear-drag model toward terminal velocity:",
            "    v(t) = Vt * (1 - e^{-(g/Vt) t})",
            "    s(t) = Vt * t - (Vt²/g) * (1 - e^{-(g/Vt) t})",
            "",
            "Tips:",
            "  • On low gravity worlds you can fall longer for the same altitude.",
            "  • Higher terminal velocity makes the ground rush faster.",
            "  • Start conservative; chase the leaderboard once you get a feel for it."
        });

        Console.WriteLine();
        Console.Write("Press any key to return...");
        Console.ReadKey(true);
    }

    static void DrawBox(string title, IEnumerable<string> lines)
    {
        int width = Math.Max(60, Math.Max(title.Length + 4, lines.Any() ? lines.Max(s => s.Length) + 4 : 4));
        string top = "╔" + new string('═', width - 2) + "╗";
        string mid = "╠" + new string('═', width - 2) + "╣";
        string bot = "╚" + new string('═', width - 2) + "╝";
        Console.WriteLine(top);
        Console.WriteLine("║ " + title.PadRight(width - 3) + "║");
        Console.WriteLine(mid);
        foreach (var s in lines)
        {
            Console.WriteLine("║ " + s.PadRight(width - 3) + "║");
        }
        Console.WriteLine(bot);
    }

    // -------- Input helpers --------
    static bool AskYesNo(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
            if (s.StartsWith("Y")) return true;
            if (s.StartsWith("N")) return false;
            Console.WriteLine("PLEASE ANSWER YES OR NO.");
        }
    }

    static double AskDouble(string prompt, double min, double max)
    {
        while (true)
        {
            Console.Write(prompt);
            var s = (Console.ReadLine() ?? "").Trim();
            if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out double v) && v >= min && v <= max)
                return v;
            Console.WriteLine($"ENTER A NUMBER BETWEEN {min} AND {max}.");
        }
    }

    static char ReadKey(string prompt, string allowedUpper)
    {
        Console.Write(prompt);
        while (true)
        {
            var k = Console.ReadKey(true).KeyChar;
            char c = char.ToUpperInvariant(k);
            if (allowedUpper.Contains(c))
            {
                Console.WriteLine(c);
                return c;
            }
        }
    }

    static void PrintBanner()
    {
        Console.WriteLine("WELCOME TO \"SPLAT\" — THE GAME THAT SIMULATES A PARACHUTE JUMP.");
        Console.WriteLine("TRY TO OPEN YOUR CHUTE AT THE LAST POSSIBLE MOMENT WITHOUT GOING SPLAT.");
        Console.WriteLine();
    }
}
