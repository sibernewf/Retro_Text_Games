using System;
using System.Globalization;

namespace Guner1;

internal static class Program
{
    static void Main()
    {
        Console.Title = "GUNER1 — Moving Target Trainer";
        new Scenario().Run();
    }
}

internal sealed class Scenario
{
    readonly Random rng = new();

    // one engagement parameters
    int maxGunRange;        // yards — varies every contact
    int targetRange;        // yards — current distance
    int speed;              // yards of random drift per shot
    int burst;              // yards — user picks

    public void Run()
    {
        if (AskYesNo("DO YOU WANT INSTRUCTIONS? "))
        {
            Console.WriteLine("\nTHIS GAME TESTS YOUR ABILITY TO HIT A MOVING TARGET.");
            Console.WriteLine("YOU MUST DESTROY IT BEFORE IT DESTROYS YOU OR MOVES OUT OF RANGE.");
            Console.WriteLine("AFTER EACH SHOT THE TARGET MOVES RANDOMLY (UNKNOWN DIRECTION).");
            Console.WriteLine("ELEVATION is the angle of your gun in degrees; maximum range occurs at 45 degrees.");
            Console.WriteLine("Type Q anytime to quit.\n");
        }

        // player settings
        speed = ReadInt("ENTER SPEED (yards per move, 10–500): ", 10, 500, allowQuit: true);
        if (speed == int.MinValue) return;

        burst = ReadInt("ENTER BURST RADIUS (yards, 20–200; 80 is suggested): ", 20, 200, allowQuit: true);
        if (burst == int.MinValue) return;

        Console.WriteLine();

        while (true)
        {
            NewContact();
            int rounds = 0;

            while (true)
            {
                Console.WriteLine($"\nTHE MAXIMUM RANGE OF YOUR GUN IS {maxGunRange:N0} YARDS");
                Console.WriteLine($"TARGET RANGE IS {targetRange:N0} YARDS");

                double elev = ReadAngle("ELEVATION? ");
                if (double.IsNaN(elev)) return;
                rounds++;

                int impact = RangeAt(maxGunRange, elev);
                int diff = impact - targetRange;

                if (Math.Abs(diff) <= burst)
                {
                    Console.WriteLine(diff == 0 ? "**** DIRECT HIT ****" : "*** TARGET DESTROYED ***");
                    Console.WriteLine($"{rounds} ROUNDS EXPENDED");
                    break; // new contact next
                }

                if (diff > 0)
                    Console.WriteLine($"OVER TARGET BY {diff:N0} YARDS");
                else
                    Console.WriteLine($"SHORT OF TARGET BY {-diff:N0} YARDS");

                // target moves: unknown direction, up to 'speed'
                int drift = rng.Next(-speed, speed + 1);
                targetRange = Math.Max(0, targetRange + drift);

                // defeat conditions like the listing vibe
                if (targetRange < 800) // got too close
                {
                    Console.WriteLine("THE TARGET HAS DESTROYED YOU!!");
                    return;
                }
                if (targetRange > (int)(maxGunRange * 1.05)) // slipped out of danger area
                {
                    Console.WriteLine("THE TARGET IS OUT OF RANGE!");
                    // either spawn new contact or end; original keeps going — we'll continue
                    break;
                }
            }

            Console.WriteLine();
            if (!AskYesNo("ANOTHER CONTACT (Y/N)? ")) break;
            Console.WriteLine();
        }
    }

    void NewContact()
    {
        // max range varies widely (see sample 27k..68k). Start target somewhere reachable.
        maxGunRange = rng.Next(27000, 68000);
        targetRange = rng.Next((int)(0.5 * maxGunRange), (int)(0.95 * maxGunRange));
    }

    static int RangeAt(int rmax, double deg)
    {
        double r = rmax * Math.Sin(2.0 * Math.PI * (deg / 180.0));
        return (int)Math.Round(Math.Max(0, r));
    }

    static double ReadAngle(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string s = (Console.ReadLine() ?? "").Trim();
            if (s.Equals("Q", StringComparison.OrdinalIgnoreCase)) return double.NaN;

            if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out double a)
                && a >= 0 && a <= 89)
                return a;

            Console.WriteLine("ENTER AN ELEVATION IN DEGREES (0–89), OR Q TO QUIT.");
        }
    }

    static int ReadInt(string prompt, int min, int max, bool allowQuit = false)
    {
        while (true)
        {
            Console.Write(prompt);
            var s = (Console.ReadLine() ?? "").Trim();
            if (allowQuit && s.Equals("Q", StringComparison.OrdinalIgnoreCase)) return int.MinValue;
            if (int.TryParse(s, out int n) && n >= min && n <= max) return n;
            Console.WriteLine($"ENTER A NUMBER FROM {min} TO {max}{(allowQuit ? " (or Q to quit)" : "")}.");
        }
    }

    static bool AskYesNo(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
            if (s is "Y" or "YES" or "1") return true;
            if (s is "N" or "NO" or "0")  return false;
        }
    }
}
