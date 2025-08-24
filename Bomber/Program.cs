using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BomberModern
{
    enum Side { Allies = 1, Germany = 2, Japan = 3, Italy = 4 }

    record Aircraft(string Name, int PayloadTons, int RangeMiles);
    record Target(string Name, string Region, double Defense); // Defense: 0(light)..0.3(very tough)

    class Program
    {
        static readonly Dictionary<int, Aircraft> Aircrafts = new()
        {
            {1, new Aircraft("B-24 Liberator", 2, 2100)},
            {2, new Aircraft("B-29 Superfortress", 5, 3250)},
            {3, new Aircraft("B-17 Flying Fortress", 2, 2000)},
            {4, new Aircraft("Avro Lancaster", 4, 1600)},
        };

        static readonly Dictionary<Side, List<Target>> Targets = new()
        {
            [Side.Allies] = new()
            {
                new Target("Ploesti Oil Fields", "Romania", 0.25),
                new Target("Ruhr Water Plant", "Germany", 0.22),
                new Target("Hamburg Docks", "Germany", 0.20),
                new Target("Bismarck (North Sea)", "North Sea", 0.18),
            },
            [Side.Germany] = new()
            {
                new Target("London", "England", 0.18),
                new Target("Convoys (North Sea)", "North Sea", 0.15),
                new Target("Moscow", "Russia", 0.20),
            },
            [Side.Japan] = new()
            {
                new Target("Hiroshima", "Japan", 0.20),
                new Target("USS Lexington", "Pacific", 0.16),
                new Target("Darwin", "Australia", 0.14),
            },
            [Side.Italy] = new()
            {
                new Target("Alexandria", "Egypt", 0.16),
                new Target("Albania Airfields", "Balkans", 0.12),
                new Target("North Africa Depot", "Libya", 0.14),
            }
        };

        static readonly Random Rng = new();

        static void Main()
        {
            Console.WriteLine("*** BOMBER ***  (Modernized)");
            Console.WriteLine("Press Q at any prompt to quit.\n");

            var log = new List<string>();

            while (true)
            {
                // Side
                Console.Write("WHAT SIDE?  (1 Allies, 2 Germany, 3 Japan, 4 Italy): ");
                var side = ReadChoice<Side>(1, 4);
                if (side.quit) break;
                var sideVal = (Side)side.value;
                log.Add($"Side: {sideVal}");

                // Aircraft
                Console.Write("AIRCRAFT — 1 Liberator, 2 B-29, 3 B-17, 4 Lancaster? ");
                var acChoice = ReadChoice<int>(1, 4);
                if (acChoice.quit) break;
                var ac = Aircrafts[acChoice.value];
                Console.WriteLine($"You’ve got {ac.PayloadTons} tons of bombs in a {ac.Name}.");
                log.Add($"Aircraft: {ac.Name}, payload {ac.PayloadTons}t");

                // Target
                var menu = Targets[sideVal];
                Console.WriteLine("\nSELECT TARGET:");
                for (int i = 0; i < menu.Count; i++)
                    Console.WriteLine($"{i + 1}. {menu[i].Name} ({menu[i].Region})");
                Console.Write("Your target? ");
                var tChoice = ReadChoice<int>(1, menu.Count);
                if (tChoice.quit) break;
                var tgt = menu[tChoice.value - 1];
                Console.WriteLine($"\nYou’re going for **{tgt.Name}** in {tgt.Region}.");
                log.Add($"Target: {tgt.Name} ({tgt.Region})");

                // Missions flown
                Console.Write("HOW MANY MISSIONS HAVE YOU FLOWN? ");
                var missions = ReadInt(0, 1000);
                if (missions.quit) break;
                Console.WriteLine(missions.value switch
                {
                    < 5 => "Fresh out of training, eh?",
                    < 25 => "That’s pushing the odds!",
                    < 75 => "A steady veteran.",
                    _ => "Old-timer! Keep those wings level."
                });

                // “Missed target by X miles!” (narrative only, like BASIC sample)
                int missBy = missions.value < 20 ? Rng.Next(5, 21) : Rng.Next(1, 6);
                Console.WriteLine($"\nMISSED TARGET BY {missBy} MILES!!");
                Console.WriteLine("NOW YOU’RE REALLY IN FOR IT!");
                log.Add($"Missed by {missBy} miles");

                // Enemy defenses
                Console.Write("\nDOES THE ENEMY HAVE 1=GUNS, 2=MISSILES, OR 3=BOTH? ");
                var weps = ReadChoice<int>(1, 3);
                if (weps.quit) break;

                Console.Write("WHAT IS THE PERCENT HIT RATE OF THE ENEMY GUNNERS (10 TO 50)? ");
                var hitPct = ReadInt(10, 50);
                if (hitPct.quit) break;

                // Outcome calculation (simple but flavorful)
                double survival = 0.75;                    // base
                survival += Math.Clamp(missions.value / 200.0, 0, 0.10); // experience helps (up to +10%)
                survival -= tgt.Defense;                   // target difficulty
                double wepPenalty = weps.value switch
                {
                    1 => 0.10,
                    2 => 0.15,
                    3 => 0.25,
                    _ => 0.10
                };
                survival -= wepPenalty * (hitPct.value / 50.0); // scale with gunners’ accuracy
                survival = Math.Clamp(survival, 0.05, 0.95);

                // Flavor for regions
                if (tgt.Region.Contains("Germany", StringComparison.OrdinalIgnoreCase) ||
                    tgt.Region.Contains("Hamburg", StringComparison.OrdinalIgnoreCase) ||
                    tgt.Region.Contains("Ruhr", StringComparison.OrdinalIgnoreCase))
                    Console.WriteLine("\nNEARING GERMANY. BE CAREFUL. THEY’VE GOT A GOOD AIR-RAID DEFENCE.");
                if (tgt.Name.Contains("Bismarck", StringComparison.OrdinalIgnoreCase))
                    Console.WriteLine("\nYOU’RE CHASING THE BISMARCK IN THE NORTH SEA.");

                // Drumroll
                Console.WriteLine("\nYou thread the flak corridor…");
                bool lived = Rng.NextDouble() < survival;

                if (lived)
                {
                    Console.WriteLine("\nYOU MADE IT THROUGH TREMENDOUS FLAK!!");
                    Console.WriteLine($"MISSION SUCCESSFUL — {ac.PayloadTons} TONS ON TARGET.");
                    log.Add("SURVIVED");
                }
                else
                {
                    Console.WriteLine("\n*** YOU’VE BEEN SHOT DOWN. ***");
                    Console.WriteLine("DEARLY BELOVED, WE ARE GATHERED HERE TODAY TO PAY OUR LAST TRIBUTE.");
                    log.Add("SHOT DOWN");
                }

                Console.Write("\nPLAY AGAIN (Y OR N)? ");
                if (!AskYes()) break;
                Console.WriteLine();
            }

            File.WriteAllLines("bomber_log.txt", log);
            Console.WriteLine($"\nLog saved to: {Path.GetFullPath("bomber_log.txt")}");
        }

        // ---------- input helpers ----------
        static (int value, bool quit) ReadChoice<T>(int lo, int hi)
        {
            while (true)
            {
                var s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
                if (s == "Q") return (0, true);
                if (int.TryParse(s, out int v) && v >= lo && v <= hi) return (v, false);
                Console.Write($"Enter a number {lo}-{hi} (Q quits): ");
            }
        }

        static (int value, bool quit) ReadInt(int lo, int hi)
        {
            while (true)
            {
                var s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
                if (s == "Q") return (0, true);
                if (int.TryParse(s, out int v) && v >= lo && v <= hi) return (v, false);
                Console.Write($"Enter an integer {lo}-{hi} (Q quits): ");
            }
        }

        static decimal ReadDecimal()
        {
            while (true)
            {
                var s = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
                if (s == "Q") Environment.Exit(0);
                if (decimal.TryParse(s, out var v)) return v;
                Console.Write("Enter a number (Q quits): ");
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
