using System;
using System.Globalization;

namespace Gunner;

internal static class Program
{
    static void Main()
    {
        Console.Title = "GUNNER — Field Artillery Trainer";
        new Game().Run();
    }
}

internal sealed class Game
{
    readonly Random rng = new();
    const int Rmax = 46500;       // yards (fixed in this version)
    const int Burst = 100;        // yards
    int ammo = 20;                // total ammunition for today

    public void Run()
    {
        Intro();

        int contact = 0;
        while (ammo > 0)
        {
            contact++;
            int target = rng.Next(9000, 44000); // starting distance
            Console.WriteLine($"\nDISTANCE TO THE TARGET IS {target} YARDS.");
            int roundsForThisTarget = 0;

            for (int shot = 1; shot <= 5; shot++)
            {
                if (ammo == 0) break;
                ammo--; roundsForThisTarget++;

                double elev = ReadAngle("ELEVATION? ");
                if (double.IsNaN(elev)) return;

                int impact = RangeAt(elev);
                int diff = impact - target;

                if (Math.Abs(diff) <= Burst)
                {
                    Console.WriteLine("***TARGET DESTROYED***   " +
                                      $"{roundsForThisTarget} ROUNDS OF AMMUNITION EXPENDED");
                    break;
                }

                if (diff > 0)
                    Console.WriteLine($"OVER TARGET BY {diff:N0} YARDS.");
                else
                    Console.WriteLine($"SHORT OF TARGET BY {-diff:N0} YARDS.");

                if (shot == 5)
                {
                    Console.WriteLine("BOOM !!!  YOU HAVE JUST BEEN DESTROYED.");
                    Console.WriteLine("SUGGEST YOU GO BACK TO FORT SILL FOR REFRESHER TRAINING!");
                }
            }

            if (ammo == 0)
            {
                Console.WriteLine("\nTOTAL ROUNDS EXPENDED ARE: 20");
                Console.WriteLine("BETTER GO BACK TO FORT SILL FOR REFRESHER TRAINING!");
                break;
            }

            // new contact appears (matches the printout vibe)
            Console.WriteLine("\nTHE FORWARD OBSERVER HAS SIGHTED MORE ENEMY ACTIVITY.");
        }

        Console.WriteLine("\nTHANK YOU FOR PLAYING.");
    }

    static int RangeAt(double degrees)
    {
        // R = Rmax · sin(2θ); degrees in, yards out
        double r = Rmax * Math.Sin(2.0 * Math.PI * (degrees / 180.0));
        return (int)Math.Round(Math.Max(0, r));
    }

    static double ReadAngle(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string s = (Console.ReadLine() ?? "").Trim();
            if (s.Equals("Q", StringComparison.OrdinalIgnoreCase)) return double.NaN;

            if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out double a) &&
                a >= 0 && a <= 89) // (open sight) realistic bound; 90 gives 0 range anyway
                return a;

            Console.WriteLine("ENTER AN ELEVATION IN DEGREES (0–89), OR Q TO QUIT.");
        }
    }

    static void Intro()
    {
        Console.WriteLine("THIS COMPUTER DEMONSTRATION SIMULATES THE RESULTS OF FIRING A FIELD ARTILLERY WEAPON.");
        Console.WriteLine("YOU ARE THE OFFICER-IN-CHARGE. TELL THE GUN CREW THE ELEVATION IN DEGREES.");
        Console.WriteLine($"MAXIMUM RANGE OF YOUR GUN IS {Rmax:N0} YARDS; BURST RADIUS IS {Burst} YARDS.");
        Console.WriteLine("HIT WITHIN THE BURST AND YOU DESTROY THE TARGET. TAKE MORE THAN 5 SHOTS — YOU'RE DESTROYED.");
        Console.WriteLine("TYPE Q AT ANY PROMPT TO QUIT.\n");
    }
}
